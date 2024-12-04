using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Habanerio.OFXSharper.Exceptions;
using Habanerio.OFXSharper.Models;
using Habanerio.OFXSharper.Types;
using Sgml;


namespace Habanerio.OFXSharper
{
    public class OFXDocumentParser
    {
        public string Version { get; private set; } = "102";

        public OFXDocument Import(FileStream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.Default))
            {
                return Import(reader.ReadToEnd());
            }
        }

        public OFXDocument Import(string ofx)
        {
            return ParseOfxDocument(ofx);
        }

        private OFXDocument ParseOfxDocument(string ofxString)
        {
            //If OFX file in SGML format, convert to XML
            if (!IsXmlVersion(ofxString))
            {
                ofxString = SGMLToXML(ofxString);
            }

            return Parse(ofxString);
        }

        private OFXDocument Parse(string ofxString)
        {
            var ofx = new OFXDocument { AccType = GetAccountType(ofxString) };

            //Load into xml document
            var doc = new XmlDocument();
            doc.Load(new StringReader(ofxString));

            var currencyNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.CURRENCY));

            if (currencyNode != null)
            {
                ofx.Currency = currencyNode.FirstChild.Value.Trim();
            }
            else
            {
                throw new OFXParseException("Currency not found");
            }

            //Get sign on node from OFX file
            var signOnNode = doc.SelectSingleNode(Resources.SignOn);

            //If exists, populate signon obj, else throw parse error
            if (signOnNode != null)
            {
                ofx.SignOn = new SignOn(signOnNode);
            }
            else
            {
                throw new OFXParseException("Sign On information not found");
            }

            //Get Account information for ofx doc
            var accountNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.ACCOUNTINFO));

            //If account info present, populate account object
            if (accountNode != null)
            {
                ofx.Account = new Account(accountNode, ofx.AccType);
            }
            else
            {
                throw new OFXParseException("Account information not found");
            }

            //Get list of transactions
            ImportTransactions(ofx, doc);

            //Get balance info from ofx doc
            var ledgerNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.BALANCE) + "/LEDGERBAL");
            var availableNode = doc.SelectSingleNode(GetXPath(ofx.AccType, OFXSection.BALANCE) + "/AVAILBAL");

            //If balance info present, populate balance object
            // ***** OFX files from my bank don't have the 'availableNode' node, so i manage a 'null' situation
            if (ledgerNode != null) // && availableNode != null
            {
                ofx.Balance = new Balance(ledgerNode, availableNode);
            }
            else
            {
                throw new OFXParseException("Balance information not found");
            }

            return ofx;
        }


        /// <summary>
        /// Returns the correct xpath to specified section for given account type
        /// </summary>
        /// <param name="type">Account type</param>
        /// <param name="section">Section of OFX document, e.g. Transaction Section</param>
        /// <exception cref="OFXException">Thrown in account type not supported</exception>
        private static string GetXPath(AccountType type, OFXSection section)
        {
            string xpath, accountInfo;

            switch (type)
            {
                case AccountType.BANK:
                    xpath = Resources.BankAccount;
                    accountInfo = "/BANKACCTFROM";
                    break;
                case AccountType.CC:
                    xpath = Resources.CCAccount;
                    accountInfo = "/CCACCTFROM";
                    break;
                default:
                    throw new OFXException("Account Type not supported. Account type " + type);
            }

            switch (section)
            {
                case OFXSection.ACCOUNTINFO:
                    return xpath + accountInfo;
                case OFXSection.BALANCE:
                    return xpath;
                case OFXSection.TRANSACTIONS:
                    return xpath + "/BANKTRANLIST";
                case OFXSection.SIGNON:
                    return Resources.SignOn;
                case OFXSection.CURRENCY:
                    return xpath + "/CURDEF";
                default:
                    throw new OFXException("Unknown section found when retrieving XPath. Section " + section);
            }
        }

        /// <summary>
        /// Returns list of all transactions in OFX document
        /// </summary>
        /// <param name="ofxDocument">OFX Document</param>
        /// <param name="doc">XML document</param>
        /// <returns>List of transactions found in OFX document</returns>
        private static void ImportTransactions(OFXDocument ofxDocument, XmlDocument doc)
        {
            var xpath = GetXPath(ofxDocument.AccType, OFXSection.TRANSACTIONS);

            ofxDocument.StatementStart = doc.GetValue(xpath + "//DTSTART")?.ToDate();
            ofxDocument.StatementEnd = doc.GetValue(xpath + "//DTEND")?.ToDate();

            var transactionNodes = doc.SelectNodes(xpath + "//STMTTRN");

            if (transactionNodes == null)
                return;

            ofxDocument.Transactions = new List<Transaction>();

            foreach (XmlNode node in transactionNodes)
                ofxDocument.Transactions.Add(new Transaction(node, ofxDocument.Currency));
        }

        /// <summary>
        /// Checks account type of supplied file
        /// </summary>
        /// <param name="file">OFX file want to check</param>
        /// <returns>Account type for account supplied in ofx file</returns>
        private static AccountType GetAccountType(string file)
        {
            if (file.IndexOf("<CREDITCARDMSGSRSV1>", StringComparison.InvariantCulture) != -1)
                return AccountType.CC;

            if (file.IndexOf("<BANKMSGSRSV1>", StringComparison.InvariantCulture) != -1)
                return AccountType.BANK;

            throw new OFXException("Unsupported Account Type");
        }

        /// <summary>
        /// Check if OFX file is in SGML or XML format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsXmlVersion(string file)
        {
            return file.IndexOf("OFXHEADER:100", StringComparison.InvariantCulture) == -1;
        }

        /// <summary>
        /// Converts SGML to XML
        /// </summary>
        /// <param name="file">OFX File (SGML Format)</param>
        /// <returns>OFX File in XML format</returns>
        private string SGMLToXML(string file)
        {
            var sgmlReader = new SgmlReader();

            //Initialize SGML reader
            var fileReader = new StringReader(ParseHeader(file));

            sgmlReader.DocType = "OFX";
            sgmlReader.InputStream = fileReader;

            // Newer implementation
            var ofxDoc = new XmlDocument();
            ofxDoc.PreserveWhitespace = false;
            ofxDoc.XmlResolver = null;
            ofxDoc.Load(sgmlReader);

            return ofxDoc.OuterXml;

            // Original implementation
            //var sw = new StringWriter();
            //var xml = new XmlTextWriter(sw);

            ////write output of sgml reader to xml text writer
            //while (!sgmlReader.EOF)
            //    xml.WriteNode(sgmlReader, true);

            ////close xml text writer
            //xml.Flush();
            //xml.Close();

            //var temp = sw.ToString().Replace("\t", "").TrimStart().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            //return string.Join("", temp);
        }

        /// <summary>
        /// Checks that the file is supported by checking the header. Removes the header.
        /// </summary>
        /// <param name="file">OFX file</param>
        /// <returns>File, without the header</returns>
        private string ParseHeader(string file)
        {
            //Select header of file and split into array
            //End of header worked out by finding first instance of '<'
            //Array split based of new line & carrige return
            var header = file.Substring(0, file.IndexOf('<'))
               .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            //Check that no errors in header
            CheckHeader(header);

            //Remove header
            return file.Substring(file.IndexOf('<')).Trim();
        }

        /// <summary>
        /// Checks that all the elements in the header are supported
        /// </summary>
        /// <param name="header">Header of OFX file in array</param>
        private void CheckHeader(string[] header)
        {
            if (header[0] == "OFXHEADER:100DATA:OFXSGMLVERSION:102SECURITY:NONEENCODING:USASCIICHARSET:1252COMPRESSION:NONEOLDFILEUID:NONENEWFILEUID:NONE")//non delimited header
                return;

            if (header[0] != "OFXHEADER:100")
                throw new OFXParseException("Incorrect header format");

            if (header[1] != "DATA:OFXSGML")
                throw new OFXParseException("Data type unsupported: " + header[1] + ". OFXSGML required");

            if (header[2].Contains("VERSION"))
            {
                Version = header[2].Split(':')[1];
            }

            //if (header[2] != "VERSION:102")
            //    throw new OFXParseException("OFX version unsupported. " + header[2]);

            // Do we care if the SECURITY is not NONE?
            //if (header[3] != "SECURITY:NONE")
            //    throw new OFXParseException("OFX security unsupported");

            if (header[4] != "ENCODING:USASCII")
                throw new OFXParseException("ASCII Format unsupported:" + header[4]);

            if (header[5] != "CHARSET:1252")
                throw new OFXParseException("Charecter set unsupported:" + header[5]);

            if (header[6] != "COMPRESSION:NONE")
                throw new OFXParseException("Compression unsupported");

            if (header[7] != "OLDFILEUID:NONE")
                throw new OFXParseException("OLDFILEUID incorrect");
        }

        #region Nested type: OFXSection

        /// <summary>
        /// Section of OFX Document
        /// </summary>
        private enum OFXSection
        {
            SIGNON,
            ACCOUNTINFO,
            TRANSACTIONS,
            BALANCE,
            CURRENCY
        }

        #endregion
    }
}