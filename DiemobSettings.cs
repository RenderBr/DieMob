using Auxiliary.Configuration;
using System.Text.Json.Serialization;

namespace DieMob
{
	public class DiemobSettings : ISettings
	{
		[JsonPropertyName("UpdateInterval")]
		public int UpdateInterval { get; set; }

		[JsonPropertyName("RepelPowerModifier")]
		public float RepelPowerModifier { get; set; }

		[JsonPropertyName("RegionStretch")]
		public int RegionStretch { get; set; }
	}
}
