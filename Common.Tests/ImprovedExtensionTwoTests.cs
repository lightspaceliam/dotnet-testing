using Hl7.Fhir.Model;

namespace Common.Tests;

public class ImprovedExtensionTwoTests
{
	[Fact]
	public void GivenMetaIsNull_Null()
	{
		var detectedIssue = new DetectedIssue
		{
			Id = "no-meta"
		};

		var response = detectedIssue.ImprovedExtensionTwo();
		
		Assert.Null(response);
	}
	
	[Fact]
	public void GivenMetaHasNoTags_Null()
	{
		var detectedIssue = new DetectedIssue
		{
			Id = "meta-has-no-tags",
			Meta = new Meta
			{
				LastUpdated = new DateTimeOffset()
			}
		};

		var response = detectedIssue.ImprovedExtensionTwo();
		
		Assert.Null(response);
	}
	
	[Fact]
	public void GivenCriteriaOneFalse_Null()
	{
		var dateTimeWithUtcOffset = "2024-06-14T20:00:00.000+00:00";
		
		var detectedIssue = new DetectedIssue
		{
			Id = "meta-has-no-tags",
			Meta = new Meta
			{
				Tag = new List<Coding>
				{
					new Coding(system:Constants.UrlTagOne, code:"false"),
					new Coding(system:Constants.UrlTagTwo, code:dateTimeWithUtcOffset)
				}
			}
		};

		var response = detectedIssue.ImprovedExtensionTwo();
		
		Assert.Null(response);
	}
	
	[Fact]
	public void GivenCriteriaTwoNotExist_Null()
	{
		var dateTimeWithUtcOffset = "2024-06-14T20:00:00.000+00:00";
		
		var detectedIssue = new DetectedIssue
		{
			Id = "meta-has-no-tags",
			Meta = new Meta
			{
				Tag = new List<Coding>
				{
					new Coding(system:Constants.UrlTagOne, code:"false"),
					new Coding(system:"https://my-url.com/criteria-other", code:dateTimeWithUtcOffset)
				}
			}
		};

		var response = detectedIssue.ImprovedExtensionTwo();
		
		Assert.Null(response);
	}
	
	[Fact]
	public void GivenExpectedData_DateTimeOffset()
	{
		var dateTimeWithUtcOffset = "2024-06-14T20:00:00.000+00:00";
		var timeZoneId = "Australia/Hobart";
		var detectedIssue = new DetectedIssue
		{
			Id = "meta-has-no-tags",
			Meta = new Meta
			{
				Tag = new List<Coding>
				{
					new Coding(system:Constants.UrlTagOne, code:"true"),
					new Coding(system:Constants.UrlTagTwo, code:dateTimeWithUtcOffset)
				}
			}
		};

		var response = detectedIssue.ImprovedExtensionTwo(timeZoneId);
		
		Assert.NotNull(response);
		var formattedResponse = response?.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
		
		var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
		if (timeZoneInfo.IsDaylightSavingTime(response!.Value))
		{
			Assert.Equal("2024-06-15T07:00:00.000+11:00", formattedResponse);
		}
		else
		{
			Assert.Equal("2024-06-15T06:00:00.000+10:00", formattedResponse);	
		}
	}
	
	[Fact]
	public void GivenSystemUrlsAreMixedCaseButCorrect_DateTimeOffset()
	{
		var dateTimeWithUtcOffset = "2024-06-14T20:00:00.000+00:00";
		var timeZoneId = "Australia/Hobart";
		var detectedIssue = new DetectedIssue
		{
			Id = "meta-has-no-tags",
			Meta = new Meta
			{
				Tag = new List<Coding>
				{
					new Coding(system:"HTTPS://my-url.com/criteRia-one", code:"TRUE"),
					new Coding(system:"HTTPS://my-Url.com/criteria-tWo", code:dateTimeWithUtcOffset)
				}
			}
		};

		var response = detectedIssue.ImprovedExtensionTwo(timeZoneId);
		
		Assert.NotNull(response);
		var formattedResponse = response?.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
		
		var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
		if (timeZoneInfo.IsDaylightSavingTime(response!.Value))
		{
			Assert.Equal("2024-06-15T07:00:00.000+11:00", formattedResponse);
		}
		else
		{
			Assert.Equal("2024-06-15T06:00:00.000+10:00", formattedResponse);	
		}
	}
	
	/// <summary>
	/// Coding is: { string, string }. Test what happens if a malformed datetiemoffsset or unexpected value is provided. 
	/// </summary>
	[Fact]
	public void GivenMalformedDateTimeOffsetOrUnexpectedValue_DateTimeOffset()
	{
		var detectedIssue = new DetectedIssue
		{
			Id = "meta-has-no-tags",
			Meta = new Meta
			{
				Tag = new List<Coding>
				{
					new Coding(system:Constants.UrlTagOne, code:"true"),
					new Coding(system:Constants.UrlTagTwo, code:"malformed-datetime-offset")
				}
			}
		};

		var response = detectedIssue.ImprovedExtensionTwo();
		
		Assert.Null(response);
	}
}