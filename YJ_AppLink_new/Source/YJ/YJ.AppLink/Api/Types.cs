using System;

namespace YJ.AppLink.Api
{
	/// <summary>
	/// For internal SDK use:
    /// Provides an static method to get an array of API types 
    /// for serialization and deserialization of messages
	/// </summary>
	internal class Types
	{
		private static Type[] types;
		
		private Types() {}

        internal static Type[] GetTypes()
		{
			if (Types.types == null)
			{
				Types.types = new Type[] 
				{ 
					typeof(Envelope),
					typeof(KeyValue),
					typeof(PriceUpdate),
					typeof(MarketDetail),
                    typeof(Quote),
                    typeof(Participant),
                    typeof(Underlying),
					typeof(StockLeg),
					typeof(SwapLeg),
					typeof(OptionLeg),
					typeof(PriceResponse),
					typeof(LegValues),
                    typeof(SendResponse),
                    typeof(SendOrder),
                    typeof(SendMessage),
					typeof(StartServiceRequest),
					typeof(StopServiceRequest),
				};
			}

			return Types.types;
		}
	}
}
