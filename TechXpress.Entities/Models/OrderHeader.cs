using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Entities.Models
{
	public class OrderHeader
	{
		public int Id { get; set; }

		public string ApplicationUserId { get; set; }

		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; }

		public DateTime OrderDate { get; set; }
		public DateTime ShippingDate { get; set; }

		public decimal TotalPrice { get; set; }

		public string? OrderStatus { get; set; }
		public string? PaymentStatus { get; set; }

		public string? TrackingNumber { get; set; }
		public string? Carrier { get; set; }

		public DateTime PaymentDate { get; set; }

		//Stripe Properties

		public string? SessionId { get; set; }
		public string? PaymentIntentId { get; set; }

		//User Data
		public string Name { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string? PhoneNumber { get; set; }

	}
}
