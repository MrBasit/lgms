﻿namespace LGMS.Dto
{
    public class BankAccountEditModel
    {
        public int Id { get; set; }
        public string AccountTitle { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string? IBAN { get; set; }
    }
}
