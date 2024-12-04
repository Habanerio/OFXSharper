using System;
using System.Globalization;
using System.Xml;
using Habanerio.OFXSharper.Exceptions;

namespace Habanerio.OFXSharper.Models
{
    public class Balance
    {
        public decimal LedgerBalance { get; set; }

        public DateTime? LedgerBalanceDate { get; set; }

        public decimal AvailableBalance { get; set; }

        public DateTime? AvailableBalanceDate { get; set; } = null;

        public Balance(XmlNode ledgerNode, XmlNode availableNode)
        {
            var tempLedgerBalance = ledgerNode.GetValue("//LEDGERBAL//BALAMT");

            if (!string.IsNullOrEmpty(tempLedgerBalance))
            {
                // ***** Forced Invariant Culture. 
                // If you don't force it, it will use the computer's default (defined in windows control panel, regional settings)
                // So, if the number format of the computer in use it's different from OFX standard (i suppose the english/invariant), 
                // the next line of could crash or (worse) the number would be wrongly interpreted. 
                // For example, my computer has a brazilian regional setting, with "." as thousand separator and "," as 
                // decimal separator, so the value "10.99" (ten 'dollars' (or whatever currency) and ninety-nine cents) would be interpreted as "1099" 
                // (one thousand and ninety-nine dollars - the "." would be ignored)
                LedgerBalance = Convert.ToDecimal(tempLedgerBalance, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new OFXParseException("Ledger balance has not been set");
            }

            // ***** OFX files from my bank don't have the 'availableNode' node, so i manage a null situation
            if (availableNode == null)
            {
                AvailableBalance = 0;
            }
            else
            {
                // *** Need to prepend `//AVAILBAL` to the xpath to get the correct value. Or else it returns the value in `//LEDGERBAL//BALAMT`
                var tempAvailableBalance = availableNode.GetValue("//AVAILBAL//BALAMT");

                if (!string.IsNullOrEmpty(tempAvailableBalance))
                {
                    // ***** Forced Invariant Culture. (same comment as above)
                    AvailableBalance = Convert.ToDecimal(tempAvailableBalance, CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new OFXParseException("Available balance has not been set");
                }

                AvailableBalanceDate = availableNode.GetValue("//DTASOF").ToDate();
            }

            LedgerBalanceDate = ledgerNode.GetValue("//DTASOF")?.ToDate();
        }
    }
}