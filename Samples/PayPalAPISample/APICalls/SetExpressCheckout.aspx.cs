using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using PayPal.Manager;
using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;

// The SetExpressCheckout API operation initiates an Express Checkout transaction.
namespace PayPalAPISample.APICalls
{
    public partial class SetExpressCheckout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // How you want to obtain payment. When implementing parallel payments, 
		    // this field is required and must be set to Order.
			// When implementing digital goods, this field is required and must be set to Sale.
			// If the transaction does not include a one-time purchase, this field is ignored. 
			// It is one of the following values:
        	// Sale – This is a final sale for which you are requesting payment (default).
    		// Authorization – This payment is a basic authorization subject to settlement with PayPal Authorization and Capture.
    	    // Order – This payment is an order authorization subject to settlement with PayPal Authorization and Capture.
			if(Request.Params.Get("paymentAction") != null )
            {
                paymentAction.SelectedValue = Request.Params.Get("paymentAction");                
            }

            // In this case, you can specify up to ten billing agreements. 
            // Other defined values are not valid.
            // Type of billing agreement for reference transactions. 
            // You must have permission from PayPal to use this field. 
            // This field must be set to one of the following values:
            // 1. MerchantInitiatedBilling - PayPal creates a billing agreement 
            //    for each transaction associated with buyer.You must specify 
            //    version 54.0 or higher to use this option.
            // 2. MerchantInitiatedBillingSingleAgreement - PayPal creates a 
            //    single billing agreement for all transactions associated with buyer.
            //    Use this value unless you need per-transaction billing agreements. 
            //    You must specify version 58.0 or higher to use this option.

            if (Request.Params.Get("billingType") != null)
            {
                billingType.SelectedValue = Request.Params.Get("billingType");
            }
          
            //string requestUrl = Request.Url.OriginalString;
            //string authority = Request.Url.Authority;
            //string dnsSafeHost = Request.Url.DnsSafeHost;

            //if (Request.UrlReferrer != null && Request.UrlReferrer.Scheme == "https")
            //{
            //    requestUrl = requestUrl.Replace("http://", "https://");
            //    requestUrl = requestUrl.Replace(authority, dnsSafeHost);
            //}

            string requestUrl = ConfigurationManager.AppSettings["HOSTING_ENDPOINT"].ToString();

            // (Required) URL to which the buyer's browser is returned after choosing to pay with PayPal. For digital goods, you must add JavaScript to this page to close the in-context experience.
            // Note:
            // PayPal recommends that the value be the final review page on which the buyer confirms the order and payment or billing agreement.
            UriBuilder uriBuilder = new UriBuilder(requestUrl);
            uriBuilder.Path = Request.ApplicationPath  
                + (Request.ApplicationPath.EndsWith("/") ? "" : "/")
                + "APICalls/GetExpressCheckoutDetails.aspx";
            returnUrl.Value = uriBuilder.Uri.ToString();

           //(Required) URL to which the buyer is returned if the buyer does not approve the use of PayPal to pay you. For digital goods, you must add JavaScript to this page to close the in-context experience.
           // Note:
           // PayPal recommends that the value be the original page on which the buyer chose to pay with PayPal or establish a billing agreement.
            uriBuilder = new UriBuilder(requestUrl);
            uriBuilder.Path = Request.ApplicationPath
                + (Request.ApplicationPath.EndsWith("/") ? "" : "/")
                + "APICalls/SetExpressCheckout.aspx";                
            cancelUrl.Value = uriBuilder.Uri.ToString();
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            // Create request object
            SetExpressCheckoutRequestType request = new SetExpressCheckoutRequestType();
            populateRequestObject(request);

            // Invoke the API
            SetExpressCheckoutReq wrapper = new SetExpressCheckoutReq();
            wrapper.SetExpressCheckoutRequest = request;
            // Create the PayPalAPIInterfaceServiceService service object to make the API call
            PayPalAPIInterfaceServiceService service = new PayPalAPIInterfaceServiceService();
            // # API call 
            // Invoke the SetExpressCheckout method in service wrapper object  
            SetExpressCheckoutResponseType setECResponse = service.SetExpressCheckout(wrapper);

            // Check for API return status
            HttpContext CurrContext = HttpContext.Current;
            CurrContext.Items.Add("paymentDetails", request.SetExpressCheckoutRequestDetails.PaymentDetails);
            setKeyResponseObjects(service, setECResponse);
        }

        private void populateRequestObject(SetExpressCheckoutRequestType request)
        {
            SetExpressCheckoutRequestDetailsType ecDetails = new SetExpressCheckoutRequestDetailsType();
            if(returnUrl.Value != "")
            {
                ecDetails.ReturnURL = returnUrl.Value;
            }
            if(cancelUrl.Value != "")
            {
                ecDetails.CancelURL = cancelUrl.Value;
            }
            // (Optional) Email address of the buyer as entered during checkout. PayPal uses this value to pre-fill the PayPal membership sign-up portion on the PayPal pages.
            if (buyerEmail.Value != "")
            {
                ecDetails.BuyerEmail = buyerEmail.Value;
            }

            // Fix for release
            // NOTE: Setting this field overrides the setting you specified in your Merchant Account Profile
            // Indicates whether or not you require the buyer's shipping address on 
            // file with PayPal be a confirmed address. For digital goods, 
            // this field is required, and you must set it to 0. It is one of the following values:
            //  0 – You do not require the buyer's shipping address be a confirmed address.
            //  1 – You require the buyer's shipping address be a confirmed address.
            //  Note:
            //  Setting this field overrides the setting you specified in your Merchant Account Profile.
            //  Character length and limitations: 1 single-byte numeric character
            if (reqConfirmShipping.SelectedIndex != 0)
            {
                ecDetails.ReqConfirmShipping = reqConfirmShipping.SelectedValue;
            }

            // (Optional) Determines whether or not the PayPal pages should 
            //display the shipping address set by you in this SetExpressCheckout request,
            // not the shipping address on file with PayPal for this buyer. Displaying 
            // the PayPal street address on file does not allow the buyer to edit that address. 
            // It is one of the following values:
            //  0 – The PayPal pages should not display the shipping address.
            //  1 – The PayPal pages should display the shipping address.
            // Character length and limitations: 1 single-byte numeric character
            if (addressoverride.SelectedIndex != 0)
            {
                ecDetails.AddressOverride = addressoverride.SelectedValue;
            }            

            if (noShipping.SelectedIndex != 0)
            {
                ecDetails.NoShipping = noShipping.SelectedValue;
            }
            if (solutionType.SelectedIndex != 0)
            {
                ecDetails.SolutionType = (SolutionTypeType)
                    Enum.Parse(typeof(SolutionTypeType), solutionType.SelectedValue);
            }

            /* Populate payment requestDetails. 
             * SetExpressCheckout allows parallel payments of upto 10 payments. 
             * This samples shows just one payment.
             */
            PaymentDetailsType paymentDetails = new PaymentDetailsType();
            ecDetails.PaymentDetails.Add(paymentDetails);
            // (Required) Total cost of the transaction to the buyer. If shipping cost and tax charges are known, include them in this value. If not, this value should be the current sub-total of the order. If the transaction includes one or more one-time purchases, this field must be equal to the sum of the purchases. Set this field to 0 if the transaction does not include a one-time purchase such as when you set up a billing agreement for a recurring payment that is not immediately charged. When the field is set to 0, purchase-specific fields are ignored.
            double orderTotal = 0.0;
            // Sum of cost of all items in this order. For digital goods, this field is required.
            double itemTotal = 0.0;
            CurrencyCodeType currency = (CurrencyCodeType)
                Enum.Parse(typeof(CurrencyCodeType), currencyCode.SelectedValue);
            // (Optional) Total shipping costs for this order.
			// Note:
			// You must set the currencyID attribute to one of the 3-character currency codes 
			// for any of the supported PayPal currencies.
			// Character length and limitations: 
			// Value is a positive number which cannot exceed $10,000 USD in any currency.
			// It includes no currency symbol. 
			// It must have 2 decimal places, the decimal separator must be a period (.), 
			// and the optional thousands separator must be a comma (,)
            if (shippingTotal.Value != "")
            {
                paymentDetails.ShippingTotal = new BasicAmountType(currency, shippingTotal.Value);
                orderTotal += Double.Parse(shippingTotal.Value);
            }
            //(Optional) Total shipping insurance costs for this order. 
            // The value must be a non-negative currency amount or null if you offer insurance options.
            // Note:
            // You must set the currencyID attribute to one of the 3-character currency 
            // codes for any of the supported PayPal currencies.
            // Character length and limitations: 
            // Value is a positive number which cannot exceed $10,000 USD in any currency. 
            // It includes no currency symbol. It must have 2 decimal places,
            // the decimal separator must be a period (.), 
            // and the optional thousands separator must be a comma (,).
            // InsuranceTotal is available since version 53.0.
            if (insuranceTotal.Value != "" && !double.Parse(insuranceTotal.Value).Equals(0.0))
            {
                paymentDetails.InsuranceTotal = new BasicAmountType(currency, insuranceTotal.Value);                
                paymentDetails.InsuranceOptionOffered = "true";
                orderTotal += Double.Parse(insuranceTotal.Value);
            }
            //(Optional) Total handling costs for this order.
            // Note:
            // You must set the currencyID attribute to one of the 3-character currency codes 
            // for any of the supported PayPal currencies.
            // Character length and limitations: Value is a positive number which 
            // cannot exceed $10,000 USD in any currency.
            // It includes no currency symbol. It must have 2 decimal places, 
            // the decimal separator must be a period (.), and the optional 
            // thousands separator must be a comma (,). 
            if (handlingTotal.Value != "")
            {
                paymentDetails.HandlingTotal = new BasicAmountType(currency, handlingTotal.Value);
                orderTotal += Double.Parse(handlingTotal.Value);
            }
            //(Optional) Sum of tax for all items in this order.
            // Note:
            // You must set the currencyID attribute to one of the 3-character currency codes
            // for any of the supported PayPal currencies.
            // Character length and limitations: Value is a positive number which 
            // cannot exceed $10,000 USD in any currency. It includes no currency symbol.
            // It must have 2 decimal places, the decimal separator must be a period (.),
            // and the optional thousands separator must be a comma (,).
            if (taxTotal.Value != "")
            {
                paymentDetails.TaxTotal = new BasicAmountType(currency, taxTotal.Value);
                orderTotal += Double.Parse(taxTotal.Value);
            }
            //(Optional) Description of items the buyer is purchasing.
            // Note:
            // The value you specify is available only if the transaction includes a purchase.
            // This field is ignored if you set up a billing agreement for a recurring payment 
            // that is not immediately charged.
            // Character length and limitations: 127 single-byte alphanumeric characters
            if (orderDescription.Value != "")
            {
                paymentDetails.OrderDescription = orderDescription.Value;
            }
            // How you want to obtain payment. When implementing parallel payments, 
            // this field is required and must be set to Order.
            // When implementing digital goods, this field is required and must be set to Sale.
            // If the transaction does not include a one-time purchase, this field is ignored. 
            // It is one of the following values:
            //   Sale – This is a final sale for which you are requesting payment (default).
            //   Authorization – This payment is a basic authorization subject to settlement with PayPal Authorization and Capture.
            //   Order – This payment is an order authorization subject to settlement with PayPal Authorization and Capture.
            paymentDetails.PaymentAction = (PaymentActionCodeType)
                Enum.Parse(typeof(PaymentActionCodeType), paymentAction.SelectedValue);
            
            if (shippingName.Value != "" && shippingStreet1.Value != "" 
                && shippingCity.Value != "" && shippingState.Value != "" 
                && shippingCountry.Value != "" && shippingPostalCode.Value != "")
            {
                AddressType shipAddress = new AddressType();
                // Person's name associated with this shipping address.
				// It is required if using a shipping address.
				// Character length and limitations: 32 single-byte characters
                shipAddress.Name = shippingName.Value;
                //First street address. It is required if using a shipping address.
                //Character length and limitations: 100 single-byte characters
                shipAddress.Street1 = shippingStreet1.Value;
                //(Optional) Second street address.
                //Character length and limitations: 100 single-byte characters
                shipAddress.Street2 = shippingStreet2.Value;
                //Name of city. It is required if using a shipping address.
                //Character length and limitations: 40 single-byte characters
                shipAddress.CityName = shippingCity.Value;
                // State or province. It is required if using a shipping address.
				// Character length and limitations: 40 single-byte characters
                shipAddress.StateOrProvince = shippingState.Value;
                // Country code. It is required if using a shipping address.
				//  Character length and limitations: 2 single-byte characters
                shipAddress.Country = (CountryCodeType)
                    Enum.Parse(typeof(CountryCodeType), shippingCountry.Value);
                // U.S. ZIP code or other country-specific postal code. 
				// It is required if using a U.S. shipping address and may be
				// required for other countries.
				// Character length and limitations: 20 single-byte characters
                shipAddress.PostalCode = shippingPostalCode.Value;

                //Fix for release
                shipAddress.Phone = shippingPhone.Value;

                ecDetails.PaymentDetails[0].ShipToAddress = shipAddress;
            }
            
            // Each payment can include requestDetails about multiple items
            // This example shows just one payment item
            if (itemName.Value != null && itemAmount.Value != null && itemQuantity.Value != null)
            {
                PaymentDetailsItemType itemDetails = new PaymentDetailsItemType();
                itemDetails.Name = itemName.Value;
                itemDetails.Amount = new BasicAmountType(currency, itemAmount.Value);
                itemDetails.Quantity = Int32.Parse(itemQuantity.Value);
                // Indicates whether an item is digital or physical. For digital goods, this field is required and must be set to Digital. It is one of the following values:
                //   1.Digital
                //   2.Physical
                //  This field is available since version 65.1. 
                itemDetails.ItemCategory = (ItemCategoryType)
                    Enum.Parse(typeof(ItemCategoryType), itemCategory.SelectedValue);
                itemTotal += Double.Parse(itemDetails.Amount.value) * itemDetails.Quantity.Value;
                //(Optional) Item sales tax.
                //    Note: You must set the currencyID attribute to one of 
                //    the 3-character currency codes for any of the supported PayPal currencies.
                //    Character length and limitations: Value is a positive number which cannot exceed $10,000 USD in any currency.
                //    It includes no currency symbol. It must have 2 decimal places, the decimal separator must be a period (.), 
                //    and the optional thousands separator must be a comma (,).
                if (salesTax.Value != "")
                {
                    itemDetails.Tax = new BasicAmountType(currency, salesTax.Value);
                    orderTotal += Double.Parse(salesTax.Value);
                }
                //(Optional) Item description.
                // Character length and limitations: 127 single-byte characters
                // This field is introduced in version 53.0. 
                if (itemDescription.Value != "")
                {
                    itemDetails.Description = itemDescription.Value;
                }
                paymentDetails.PaymentDetailsItem.Add(itemDetails);                
            }

            orderTotal += itemTotal;
            paymentDetails.ItemTotal = new BasicAmountType(currency, itemTotal.ToString());
            paymentDetails.OrderTotal = new BasicAmountType(currency, orderTotal.ToString());

            //(Optional) Your URL for receiving Instant Payment Notification (IPN) 
            //about this transaction. If you do not specify this value in the request, 
            //the notification URL from your Merchant Profile is used, if one exists.
            //Important:
            //The notify URL applies only to DoExpressCheckoutPayment. 
            //This value is ignored when set in SetExpressCheckout or GetExpressCheckoutDetails.
            //Character length and limitations: 2,048 single-byte alphanumeric characters
            paymentDetails.NotifyURL = ipnNotificationUrl.Value.Trim();
            
            // Set Billing agreement (for Reference transactions & Recurring payments)
            if (billingAgreementText.Value != "")
            {
                 //(Required) Type of billing agreement. For recurring payments,
                 //this field must be set to RecurringPayments. 
                 //In this case, you can specify up to ten billing agreements. 
                 //Other defined values are not valid.
                 //Type of billing agreement for reference transactions. 
                 //You must have permission from PayPal to use this field. 
                 //This field must be set to one of the following values:
                 //   1. MerchantInitiatedBilling - PayPal creates a billing agreement 
                 //      for each transaction associated with buyer.You must specify 
                 //      version 54.0 or higher to use this option.
                 //   2. MerchantInitiatedBillingSingleAgreement - PayPal creates a 
                 //      single billing agreement for all transactions associated with buyer.
                 //      Use this value unless you need per-transaction billing agreements. 
                 //      You must specify version 58.0 or higher to use this option.
                BillingCodeType billingCodeType = (BillingCodeType)
                    Enum.Parse(typeof(BillingCodeType), billingType.SelectedValue);
                BillingAgreementDetailsType baType = new BillingAgreementDetailsType(billingCodeType);
                baType.BillingAgreementDescription = billingAgreementText.Value;
                ecDetails.BillingAgreementDetails.Add(baType);
            }

            //(Optional) Locale of pages displayed by PayPal during Express Checkout.           
            if (localeCode.SelectedIndex != 0)
            {
                ecDetails.LocaleCode = localeCode.SelectedValue;
            }

            // (Optional) Name of the Custom Payment Page Style for payment pages associated with this button or link. It corresponds to the HTML variable page_style for customizing payment pages. It is the same name as the Page Style Name you chose to add or edit the page style in your PayPal Account profile.
            if (pageStyle.Value != "")
            {
                ecDetails.PageStyle = pageStyle.Value;
            }
            // (Optional) URL for the image you want to appear at the top left of the payment page. The image has a maximum size of 750 pixels wide by 90 pixels high. PayPal recommends that you provide an image that is stored on a secure (https) server. If you do not specify an image, the business name displays.
            if (cppheaderimage.Value != "")
            {
                ecDetails.cppHeaderImage = cppheaderimage.Value;
            }
            // (Optional) Sets the border color around the header of the payment page. The border is a 2-pixel perimeter around the header space, which is 750 pixels wide by 90 pixels high. By default, the color is black.
            if (cppheaderbordercolor.Value != "")
            {
                ecDetails.cppHeaderBorderColor = cppheaderbordercolor.Value;
            }
            // (Optional) Sets the background color for the header of the payment page. By default, the color is white.
            if (cppheaderbackcolor.Value != "")
            {
                ecDetails.cppHeaderBackColor = cppheaderbackcolor.Value;
            }
            // (Optional) Sets the background color for the payment page. By default, the color is white.
            if (cpppayflowcolor.Value != "")
            {
                ecDetails.cppPayflowColor = cpppayflowcolor.Value;
            }
            // (Optional) A label that overrides the business name in the PayPal account on the PayPal hosted checkout pages.
            if (brandName.Value != "")
            {
                ecDetails.BrandName = brandName.Value;
            }

            request.SetExpressCheckoutRequestDetails = ecDetails;
        }


        // A helper method used by APIResponse.aspx that returns select response parameters 
        // of interest. 
        private void setKeyResponseObjects(PayPalAPIInterfaceServiceService service, SetExpressCheckoutResponseType setECResponse)
        {
            Dictionary<string, string> keyResponseParameters = new Dictionary<string, string>();
            keyResponseParameters.Add("API Status", setECResponse.Ack.ToString());
            HttpContext CurrContext = HttpContext.Current;
            if (setECResponse.Ack.Equals(AckCodeType.FAILURE) ||
                (setECResponse.Errors != null && setECResponse.Errors.Count > 0))
            {
                CurrContext.Items.Add("Response_error", setECResponse.Errors);
                CurrContext.Items.Add("Response_redirectURL", null);
            }
            else
            {
                CurrContext.Items.Add("Response_error", null);
                keyResponseParameters.Add("EC token", setECResponse.Token);
                CurrContext.Items.Add("Response_redirectURL", ConfigurationManager.AppSettings["PAYPAL_REDIRECT_URL"].ToString()
                    + "_express-checkout&token=" + setECResponse.Token);
            }            
            CurrContext.Items.Add("Response_keyResponseObject", keyResponseParameters);
            CurrContext.Items.Add("Response_apiName", "SetExpressCheckout");
            CurrContext.Items.Add("Response_requestPayload", service.getLastRequest());
            CurrContext.Items.Add("Response_responsePayload", service.getLastResponse());
            Server.Transfer("../APIResponse.aspx");
        }
    }
}
