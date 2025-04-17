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

namespace AutoCounter3
{
    public class Class1 : MelonMod
    {
        private ConfigManager configManager;
        private float autoCounterTimer = 0f;
        public override void OnEarlyInitializeMelon()
        {
            try
            {
                base.OnEarlyInitializeMelon();

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

        private void ExecuteLogic()
        {
            var unlockedCustomers = Customer.UnlockedCustomers;
            if (unlockedCustomers == null || unlockedCustomers.Count == 0) return;

            var activeConversations = new List<MSGConversation>();
            foreach (var customer in unlockedCustomers)
            {
                var msg = customer?.NPC?.MSGConversation;
                if (msg != null && msg.AreResponsesActive && !activeConversations.Contains(msg))
                {
                    activeConversations.Add(msg);
                }
            }

            MelonCoroutines.Start(MSGResponderCoroutine(activeConversations));
        }

        private IEnumerator MSGResponderCoroutine(List<MSGConversation> msgs)
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

                var customer = GetCustomerFromMessage(msg);
                if (customer == null)
                {
                    MelonLogger.Warning("Customer is null or invalid. Skipping.");
                    continue;
                }

                if (configManager.Config.EnableCounter)
                {
                    int newQuantity = RoundUp(quantity);
                    price = configManager.Config.PricePerUnit ?? customer.offeredContractInfo.Payment / quantity;

                    customer.SendCounteroffer(product, newQuantity, price * newQuantity);
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

        private (float price, int quantity, ProductDefinition product) SetCounterOfferDetails(MSGConversation msg)
        {
            var customer = GetCustomerFromMessage(msg);
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

        private Customer GetCustomerFromMessage(MSGConversation message)
        {
            if (message == null) return null;

            foreach (var customer in Customer.UnlockedCustomers)
            {
                if (customer?.NPC?.MSGConversation == message)
                {
                    return customer;
                }
            }
            return null;
        }

        private void ScheduleDealTime(Customer customer)
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
        private void AcceptContract(Customer customer)
        {
            if (configManager.Config.choosetimemanual == true) return;

            customer.PlayerAcceptedContract(configManager.Config.DealWindow);
            customer.SendContractAccepted(configManager.Config.DealWindow, true);
            customer.NPC.MSGConversation.SetRead(true);
        }
        private int RoundUp(int value)
        {
            return (int)(Math.Ceiling(value / (double) configManager.Config.RoundTo) * configManager.Config.RoundTo);
        }

        //private int GetBestPriceForQuantity(int quantity, Customer custumer)
        //{
        //    if (productList == null || productList.entries == null || productList.entries.Count == 0)
        //    {
        //        MelonLogger.Error("Product list is empty or invalid.");
        //        return 0;
        //    }
        //    MelonLogger.Msg($"{ Customer.GetValueProposition(product, 50)}");
        //    return 0;
        //}

        //public static float CalculateSuccessProbability(Customer customer, ProductDefinition product, int quantity, float price)
        //{
        //    float OGvalueProposition = Customer.GetValueProposition(product, customer.OfferedContractInfo.Payment / (float)customer.OfferedContractInfo.Products.GetTotalQuantity());
        //    float valueProposition = Customer.GetValueProposition(product, price / quantity);
        //    float valueDiff = valueProposition/OGvalueProposition;
        //    float baseValue = customer.GetProductEnjoyment(product, customer.CustomerData.Standards.GetCorrespondingQuality());
        //    float normalizedValue = Mathf.InverseLerp(-1f, 1f, baseValue); // Normalize to 0-1 range

        //    MelonLogger.Msg($"Debug: valueProposition = {valueProposition}, baseValue = {baseValue}, normalizedValue = {normalizedValue}, test = {Customer.GetValueProposition(product, 38)}");

        //    if (OGvalueProposition <= valueProposition)
        //        return 1f;

        //    float result = Mathf.Clamp01(valueDiff * normalizedValue);
        //    MelonLogger.Msg($"Debug: result = {result}");
        //    return result;
        //}
    }
}
