using System;
using System.Collections.Generic;
using Habanerio.OFXSharper.Models;
using Habanerio.OFXSharper.Types;

namespace Habanerio.OFXSharper
{
    public class OFXDocument
    {
        public DateTime? StatementStart { get; set; }

        public DateTime? StatementEnd { get; set; }

        public AccountType AccType { get; set; }

        public string Currency { get; set; }

        public SignOn SignOn { get; set; }

        public Account Account { get; set; }

        public Balance Balance { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}