using System;
using System.IO;
using Habanerio.OFXSharper.Types;
using Xunit;

namespace Habanerio.OFXSharper.Tests
{
    public class CanParser
    {
        [Fact]
        public void CanParserBankTransactions()
        {
            var parser = new OFXDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"bankTransactions.sgml", FileMode.Open));

            Assert.NotNull(ofxDocument);
            Assert.NotNull(ofxDocument.Account);

            Assert.Equal(AccountType.BANK, ofxDocument.AccType);

            Assert.Equal("0000000000003158", ofxDocument.Account.AccountID);
            Assert.Equal("3158", ofxDocument.Account.AccountKey);
            Assert.Equal(AccountType.BANK, ofxDocument.Account.AccountType);
            Assert.Equal(BankAccountType.CHECKING, ofxDocument.Account.BankAccountType);
            Assert.Equal("011000138", ofxDocument.Account.BankID);
            Assert.Equal("003", ofxDocument.Account.BranchID);

            Assert.NotNull(ofxDocument.Balance);
            Assert.Equal(1327.42M, ofxDocument.Balance.AvailableBalance);
            Assert.Equal(new DateTime(2024, 02, 08, 0, 0, 0
                , DateTimeKind.Unspecified), ofxDocument.Balance.AvailableBalanceDate);

            Assert.Equal(1327.42M, ofxDocument.Balance.LedgerBalance);
            Assert.Equal(new DateTime(2024, 02, 08, 0, 0, 0
                , DateTimeKind.Unspecified), ofxDocument.Balance.LedgerBalanceDate);

            Assert.Equal("USD", ofxDocument.Currency);

            Assert.NotNull(ofxDocument.SignOn);
            Assert.Equal(new DateTime(2024, 02, 09, 0, 0, 0
                , DateTimeKind.Unspecified), ofxDocument.SignOn.DTServer);
            Assert.Equal("", ofxDocument.SignOn.IntuBid);
            Assert.Equal("ENG", ofxDocument.SignOn.Language);
            Assert.Equal(0, ofxDocument.SignOn.StatusCode);
            Assert.Equal("INFO", ofxDocument.SignOn.StatusSeverity);

            Assert.Equal(new DateTime(2024, 01, 11, 0, 0, 0
                , DateTimeKind.Unspecified), ofxDocument.StatementStart);
            Assert.Equal(new DateTime(2024, 02, 06, 0, 0, 0
                , DateTimeKind.Unspecified), ofxDocument.StatementEnd);
        }

        [Fact]
        public void CanParserCreditCardTransactions()
        {
            var parser = new OFXDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"creditCardTransactions.sgml", FileMode.Open));

            Assert.NotNull(ofxDocument);
            Assert.NotNull(ofxDocument.Account);

            Assert.Equal(AccountType.CC, ofxDocument.AccType);

            Assert.Equal("XXXXXXXXXXXX3158", ofxDocument.Account.AccountID);
            Assert.Equal(string.Empty, ofxDocument.Account.AccountKey);
            Assert.Equal(AccountType.CC, ofxDocument.Account.AccountType);
            Assert.Equal(BankAccountType.NA, ofxDocument.Account.BankAccountType);
            Assert.Null(ofxDocument.Account.BankID);
            Assert.Null(ofxDocument.Account.BranchID);

            Assert.NotNull(ofxDocument.Balance);
            Assert.Equal(12000.00M, ofxDocument.Balance.AvailableBalance);
            Assert.Equal(new DateTime(2024, 01, 04, 0, 0, 0,
                DateTimeKind.Unspecified), ofxDocument.Balance.AvailableBalanceDate);

            Assert.Equal(345m, ofxDocument.Balance.LedgerBalance);
            Assert.Equal(new DateTime(2024, 01, 04, 0, 0, 0,
                DateTimeKind.Unspecified), ofxDocument.Balance.LedgerBalanceDate);

            Assert.Equal("USD", ofxDocument.Currency);

            Assert.NotNull(ofxDocument.SignOn);
            Assert.Equal(new DateTime(2024, 01, 05, 0, 0, 0,
                DateTimeKind.Unspecified), ofxDocument.SignOn.DTServer);
            Assert.Equal("", ofxDocument.SignOn.IntuBid);
            Assert.Equal("ENG", ofxDocument.SignOn.Language);
            Assert.Equal(0, ofxDocument.SignOn.StatusCode);
            Assert.Equal("INFO", ofxDocument.SignOn.StatusSeverity);

            // TODO: Fix this
            //Assert.Equal(new DateTime(2024, 01, 04, 0, 0, 0,
            //    DateTimeKind.Unspecified), ofxDocument.StatementStart);
            //Assert.Equal(new DateTime(2024, 02, 06, 0, 0, 0,
            //    DateTimeKind.Unspecified), ofxDocument.StatementEnd);
        }

        [Fact]
        public void CanParserItau()
        {
            var parser = new OFXDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"itau.ofx", FileMode.Open));

            Assert.NotNull(ofxDocument);
            Assert.NotNull(ofxDocument.Account);
        }

        [Fact]
        public void CanParserSantander()
        {
            var parser = new OFXDocumentParser();
            var ofxDocument = parser.Import(new FileStream(@"santander.ofx", FileMode.Open));

            Assert.NotNull(ofxDocument);
            Assert.NotNull(ofxDocument.Account);
        }
    }
}
