using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaPlatform.Dtos
{
    public class NewRentalDto
    {
        public int CustomerID { get; set; }
        public List<int> BookIDs { get; set; }
    }
}