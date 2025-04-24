using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.UI.Phone.Messages;
using MelonLoader;
using UnityEngine;
using Il2CppSystem;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.Product;
using Exception = System.Exception;
using Math = System.Math;
using Il2CppScheduleOne.GameTime;
using System.Collections;
using Il2CppScheduleOne.ItemFramework;
using System.Runtime.InteropServices.WindowsRuntime;
using Il2CppScheduleOne;
using static MelonLoader.MelonLaunchOptions;

namespace AutoCounter3
{
    public class AutoCounter : MelonMod
    {
        private ConfigManager configManager;
        private float autoCounterTimer = 0f;
        public override void OnInitializeMelon()
        {
            try
            {
                configManager = new ConfigManager();
                if (configManager.Config == null)
                {
                    MelonLogger.Error("Failed to load configuration. Config is null.");
                    return;
                }
                MelonLogger.Msg("Configuration loaded successfully.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"An error occurred during initialization: {ex.Message}");
            }
        }

        public override void OnUpdate()
        {
            if (configManager.Config.AutoCounterInterval != -1)
            {
                autoCounterTimer += Time.deltaTime;

                if (autoCounterTimer >= configManager.Config.AutoCounterInterval)
                {
                    autoCounterTimer = 0f;
                    ExecuteLogic();
                }
            }

            if (Input.GetKeyDown(configManager.Config.Hotkey))
            {
                ExecuteLogic();
            }
        }

        public void ExecuteLogic()
        {
            var unlockedCustomers = Customer.UnlockedCustomers;
            if (unlockedCustomers == null || unlockedCustomers.Count == 0) return;

            var activeConversations = new List<MSGConversation>();
            foreach (var customer in unlockedCustomers)
            {
                var msg = customer?.NPC?.MSGConversation;
                if (msg != null && msg.AreResponsesActive && !activeConversations.Contains(msg) && customer.offeredContractInfo != null)
                {
                    activeConversations.Add(msg);
                }
            }

            MelonCoroutines.Start(MSGResponderCoroutine(activeConversations));
        }

        public IEnumerator MSGResponderCoroutine(List<MSGConversation> msgs)
        {
            if (msgs == null || msgs.Count == 0)
            {
                MelonLogger.Msg("No active conversations to respond to.");
                yield break;
            }

            float price;
            int quantity;
            ProductDefinition product;
            foreach (var msg in msgs)
            {
                if (!msg.AreResponsesActive) continue;

                (price,quantity,product) = SetCounterOfferDetails(msg); //setCounterOfferDetails

                var customer = GetCustomerFromConversation(msg);
                if (customer == null)
                {
                    MelonLogger.Warning("Customer is null or invalid. Skipping.");
                    continue;
                }

                if (configManager.Config.EnableCounter)
                {
                    int newQuantity = RoundUp(quantity);
                    float newprice = configManager.Config.PricePerUnit * newQuantity ?? price / quantity * newQuantity;
                    (newQuantity,newprice) = getBestQuantityAndPrice(customer, product, newQuantity, newprice, configManager.Config.RoundTo);

                    customer.SendCounteroffer(product, newQuantity, newprice);
                    MelonLogger.Msg("Sent counteroffer.");

                    yield return new WaitForSeconds(1.5f);

                    ScheduleDealTime(customer);
                }
                else
                {
                    AcceptContract(customer);
                    MelonLogger.Msg("Contract accepted without counteroffer.");
                }
            }
        }

        public (float price, int quantity, ProductDefinition product) SetCounterOfferDetails(MSGConversation msg)
        {
            var customer = GetCustomerFromConversation(msg);
            if (customer?.offeredContractInfo == null)
            {
                MelonLogger.Msg("No offer details available for the customer.");
                return(0,0,null);
            }

            var offerDetails = customer.offeredContractInfo;
            float price = offerDetails.Payment;
            int quantity = offerDetails.Products.GetTotalQuantity();
            ProductDefinition product = Registry.GetItem<ProductDefinition>(offerDetails.Products.entries[0].ProductID);

            MelonLogger.Msg($"Counteroffer Details: Price = {price}, Quantity = {quantity}, Product = {offerDetails.Products.entries[0].ProductID}");
            return (price, quantity, product);
        }
        public void ScheduleDealTime(Customer customer)
        {
            if (customer?.NPC == null)
            {
                MelonLogger.Error("Cannot schedule deal time. Customer or NPC is null.");
                return;
            }

            try
            {
                AcceptContract(customer);
                MelonLogger.Msg("Contract accepted");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"An error occurred in ScheduleDealTime: {ex.Message}");
            }
        }
        public void AcceptContract(Customer customer)
        {
            if (configManager.Config.choosetimemanual == true) return;

            customer.PlayerAcceptedContract(configManager.Config.DealWindow);
            customer.SendContractAccepted(configManager.Config.DealWindow, true);
            customer.NPC.MSGConversation.SetRead(true);
        }
        public int RoundUp(int value)
        {
            return (int)(Math.Ceiling(value / (double) configManager.Config.RoundTo) * configManager.Config.RoundTo);
        }

        public (int Quantity, float Price) getBestQuantityAndPrice(Customer customer, ProductDefinition product, int quantity, float price, int roundTo)
        {
            if (customer == null)
            {
                MelonLogger.Error("Customer is null in GetBestQuantityAndPrice.");
                return (quantity, price);
            }

            if (customer.offeredContractInfo.Products?.entries == null || customer.offeredContractInfo.Products.entries.Count == 0)
            {
                MelonLogger.Error("Customer's offeredContractInfo.Products.entries is null or empty in GetBestQuantityAndPrice.");
                return (quantity, price);
            }

            if (product == null)
            {
                MelonLogger.Error("Product is null in GetBestQuantityAndPrice.");
                return (quantity, price);
            }

            float maxSpend = CalculateSpendingLimits(customer).maxSpend;
            float originalPricePerUnit = customer.offeredContractInfo.Payment / customer.offeredContractInfo.Products.entries[0].Quantity;
            float newPricePerUnit = FindOptimalPrice(customer, product, quantity, price, maxSpend) / quantity;

            int adjustedQuantity = quantity;
            float adjustedPrice = FindOptimalPrice(customer, product, quantity, price, maxSpend);

            if (configManager.Config.PricePerUnit.HasValue)
            {
                float pricePerUnit = configManager.Config.PricePerUnit.Value;
                while (pricePerUnit * quantity < maxSpend)
                {
                    adjustedQuantity = quantity;
                    quantity += roundTo;
                }
                MelonLogger.Msg($"Name: {customer.name}, Max Spend: {maxSpend}, Adjusted Price: {adjustedPrice}, New Price Per Unit: {newPricePerUnit}, Adjusted Quantity: {adjustedQuantity}");
                return (adjustedQuantity, pricePerUnit * quantity);
            }

            int maxIterations = 30;
            while (maxIterations-- > 0)
            {
                if (!configManager.Config.GoToMaxSpend && originalPricePerUnit >= newPricePerUnit) break;

                adjustedQuantity = quantity;
                quantity += roundTo;

                adjustedPrice = price;
                price = FindOptimalPrice(customer, product, quantity, price, maxSpend, configManager.Config.MinSuccessProbability);

                newPricePerUnit = price / quantity;

                if ((adjustedPrice == price || price >= maxSpend - 2) && adjustedQuantity < quantity && configManager.Config.GoToMaxSpend)
                {
                    adjustedPrice = maxSpend - 1;
                    adjustedQuantity = quantity;
                    break;
                }
            }

            if (adjustedPrice >= maxSpend) adjustedPrice = maxSpend - 1;

            if (adjustedPrice <= customer.offeredContractInfo.Payment)
            {
                MelonLogger.Msg($"Customer's original price is better; Adjusted Price: {adjustedPrice}, Customer Price: {customer.offeredContractInfo.Payment}, try to use PricePerUnit");
                adjustedPrice = customer.offeredContractInfo.Payment;
                adjustedQuantity = customer.offeredContractInfo.Products.GetTotalQuantity();
            }

            MelonLogger.Msg($"Name: {customer.name}, Max Spend: {maxSpend}, Adjusted Price: {adjustedPrice}, New Price Per Unit: {newPricePerUnit}, Adjusted Quantity: {adjustedQuantity}");
            return (adjustedQuantity, adjustedPrice);
        }

        //code from https://github.com/xyrilyn/Deal-Optimizer-Mod
        public static float CalculateSuccessProbability(Customer customer, ProductDefinition product, int quantity, float price, bool printCalcToConsole = false)
        {
            CustomerData customerData = customer.CustomerData;

            float valueProposition = Customer.GetValueProposition(Registry.GetItem<ProductDefinition>(customer.OfferedContractInfo.Products.entries[0].ProductID),
                customer.OfferedContractInfo.Payment / (float)customer.OfferedContractInfo.Products.entries[0].Quantity);
            float productEnjoyment = customer.GetProductEnjoyment(product, customerData.Standards.GetCorrespondingQuality());
            float enjoymentNormalized = Mathf.InverseLerp(-1f, 1f, productEnjoyment);
            float newValueProposition = Customer.GetValueProposition(product, price / (float)quantity);
            float quantityRatio = Mathf.Pow((float)quantity / (float)customer.OfferedContractInfo.Products.entries[0].Quantity, 0.6f);
            float quantityMultiplier = Mathf.Lerp(0f, 2f, quantityRatio * 0.5f);
            float penaltyMultiplier = Mathf.Lerp(1f, 0f, Mathf.Abs(quantityMultiplier - 1f));

            if (newValueProposition * penaltyMultiplier > valueProposition)
            {
                return 1f;
            }
            if (newValueProposition < 0.12f)
            {
                return 0f;
            }

            float customerWeightedValue = productEnjoyment * valueProposition;
            float proposedWeightedValue = enjoymentNormalized * penaltyMultiplier * newValueProposition;

            if (proposedWeightedValue > customerWeightedValue)
            {
                return 1f;
            }

            float valueDifference = customerWeightedValue - proposedWeightedValue;
            float threshold = Mathf.Lerp(0f, 1f, valueDifference / 0.2f);
            float bonus = Mathf.Lerp(0f, 0.2f, Mathf.Max(customer.CurrentAddiction, customer.NPC.RelationData.NormalizedRelationDelta));
            float thresholdMinusBonus = threshold - bonus;

            return Mathf.Clamp01((0.9f - thresholdMinusBonus) / 0.9f);
        }
        public static int FindOptimalPrice(Customer customer, ProductDefinition product, int quantity, float currentPrice, float maxSpend, float minSuccessProbability = 0.98f)
        {
            int low = (int)currentPrice;
            int high = (int)maxSpend;
            int bestFailingPrice = (int)currentPrice;
            int maxIterations = 30;
            int iterations = 0;

            //MelonLogger.Msg($"Finding optimal price for {product} with quantity {quantity} and current price {currentPrice}");

            while (iterations < maxIterations && low < high)
            {
                int mid = (low + high) / 2;
                float probability = CalculateSuccessProbability(customer, product, quantity, mid);
                bool success = probability >= minSuccessProbability;
                //MelonLogger.Msg($"[DEBUG] Iteration {iterations}: Low = {low}, High = {high}, Mid = {mid}, Probability = {probability}");

                if (success)
                {
                    //bestFailingPrice = mid;
                    low = mid + 1;
                    if (low == high)
                    {
                        bestFailingPrice = CalculateSuccessProbability(customer, product, quantity, mid + 1) > minSuccessProbability ? mid + 1 : mid;
                        
                        break;
                    }
                }
                else
                {
                    bestFailingPrice = mid;
                    high = mid;
                }
                iterations++;
            }

            MelonLogger.Msg($"Optimal price found: {bestFailingPrice}");

            return bestFailingPrice;
        }
        public static Customer GetCustomerFromConversation(MSGConversation conversation)
        {
            string contactName = conversation.contactName;
            var unlockedCustomers = Customer.UnlockedCustomers;
            return unlockedCustomers.Find((Il2CppSystem.Predicate<Customer>)((cust) =>
            {
                NPC npc = cust.NPC;
                return npc.fullName == contactName;
            }));
        }
        public static (float maxSpend, float dailyAverage) CalculateSpendingLimits(Customer customer, bool printCalcToConsole = false)
        {
            CustomerData customerData = customer.CustomerData;
            float adjustedWeeklySpend = customerData.GetAdjustedWeeklySpend(customer.NPC.RelationData.RelationDelta / 5f);
            var orderDays = customerData.GetOrderDays(customer.CurrentAddiction, customer.NPC.RelationData.RelationDelta / 5f);
            float dailyAverage = adjustedWeeklySpend / orderDays.Count;
            float maxSpend = dailyAverage * 3f;

            return (maxSpend, dailyAverage);
        }
    }
}
