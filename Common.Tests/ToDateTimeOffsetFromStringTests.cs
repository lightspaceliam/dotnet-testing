namespace Common.Tests;

public class ToDateTimeOffsetFromStringTests
{
	//  We would have to manage this???
	private static readonly string[] KnownFormats = new string[]
	{
		"MM/dd/yyyy hh:mm:ss",
		"MM/dd/yyyy hh:mm:ss tt", 
		"MM/dd/yyyy HH:mm:ss zzz"
		//  keep adding ???. I didn't find an already existing convenient list managed by .NET 
	};
	
	[Fact]
	public void GivenAuFormat_Can()
	{
		var dateTimeWithUtcOffset = "2024-06-14T20:00:00.000+00:00";

		var response = dateTimeWithUtcOffset.ToDateTimeOffsetFromString(KnownFormats);
		
		Assert.Equal(2024, response?.Year);
		Assert.Equal(06, response?.Month);
		Assert.Equal(14, response?.Day);
	}
	
	[Fact]
	public void GivenUsFormat_Can()
	{
		var dateTimeWithUtcOffset = "12/20/2024 09:00:00";

		var response = dateTimeWithUtcOffset.ToDateTimeOffsetFromString(KnownFormats);
		
		Assert.Equal(2024, response?.Year);
		Assert.Equal(12, response?.Month);
		Assert.Equal(20, response?.Day);
	}
}