using System;
using System.Collections.Generic;
using System.Text;


namespace FunctionAppInVSErnesto.Model
{
    public class ItemCola
    {
        public int ItemColaId { get; set; } //Primary Key
        public string MessageId { get; set; }
        public string Message { get; set; }
        public System.DateTime FechaEmision { get; set; }
        public System.DateTime FechaHoraRecepcion { get; set; }
    }
}