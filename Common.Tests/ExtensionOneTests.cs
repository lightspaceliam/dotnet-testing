using Hl7.Fhir.Model;

namespace Common.Tests;

public class ExtensionOneTests
{
    /// <summary>
    /// Only works with expected data.
    /// </summary>
    [Fact(DisplayName = "Expected data with valid datetime")]
    public void ExpectedData_WorksAsExpected()
    {
        var dateTimeWithOffset = "2024-06-10T09:00:00.000+00:00";
        
        var detectedIssue = new DetectedIssue
        {
            Id = "1",
            Meta = new Meta
            {
                Tag = new List<Coding>
                {
                    new Coding(system: Constants.UrlTagOne, code: "true"),
                    new Coding(system: Constants.UrlTagTwo, code: dateTimeWithOffset)
                }
            }
        };

        var response = detectedIssue.UnsafeExtensionOne();

        Assert.NotNull(response);
        /*
         * In Syd | Mel - any time zone +10 until daylight savings time needs to be considered.
         * So when I run this test in Syd | Mel during dst this unit test will fail.
         */
        var parsedDateTimeOffset = DateTimeOffset.Parse(dateTimeWithOffset);
        var expectedFormattedDatetime = parsedDateTimeOffset.AddHours(10).ToString(Constants.DateTimeFormat);
        
        Assert.Equal(expectedFormattedDatetime, response);
    }
    
    /// <summary>
    /// There is invalid data however we cannot display inaccurate date and time. 
    /// </summary>
    [Fact(DisplayName = "FAIL: Invalid date and time. Returns min date and time with offset")]
    public void UnexpectedDataInvalidDate_DateTimePlusSystemSetOffset()
    {
        var dateTimeWithOffset = "invalid-date-time-offset";
        var detectedIssue = new DetectedIssue
        {
            Id = "1",
            Meta = new Meta
            {
                Tag = new List<Coding>
                {
                    new Coding(system: Constants.UrlTagOne, code: "true"),
                    new Coding(system: Constants.UrlTagTwo, code: dateTimeWithOffset)
                }
            }
        };

        var response = detectedIssue.UnsafeExtensionOne();
        
        Assert.NotNull(response);
        /*
         * In Syd | Mel - any time zone +10 until daylight savings time needs to be considered.
         * So when I run this test in Syd | Mel during dst this unit test will fail.
         */
        var expectedFormattedDatetime = DateTime.MinValue.AddHours(10);
        Assert.Equal(expectedFormattedDatetime.ToString(Constants.DateTimeFormat), response);
    }
    
    /// <summary>
    /// There is valid data but the system url has mixed case. By escaping case sensitivity we could have had a successful result.
    /// </summary>
    [Fact(DisplayName = "FAIL: Custom Tags 1 system url is correct but not lowercase. Returns null due to using FirstOrDefault and not escaping case sensitivity")]
    public void UnexpectedDataInvalidSystem2_DateTimePlusSystemSetOffset()
    {
        var dateTimeWithOffset = "2024-06-10T09:00:00.000+00:00";
        var detectedIssue = new DetectedIssue
        {
            Id = "1",
            Meta = new Meta
            {
                Tag = new List<Coding>
                {
                    new Coding(system: "https://my-uRl.com/criteria-One", code: "true"),
                    new Coding(system: Constants.UrlTagTwo, code: dateTimeWithOffset)
                }
            }
        };
        var response = detectedIssue.UnsafeExtensionOne();
        
        Assert.Null(response);
    }
    
    /// <summary>
    /// There is valid data but the system url has mixed case. By escaping case sensitivity we could have had a successful result. 
    /// </summary>
    [Fact(DisplayName = "FAIL: Custom Tags 2 system url is correct but not lowercase Throws due to using Find and not escaping case sensitivity")]
    public void UnexpectedDataInvalidSystem_DateTimePlusSystemSetOffset()
    {
        var dateTimeWithOffset = "2024-06-10T09:00:00.000+00:00";
        var detectedIssue = new DetectedIssue
        {
            Id = "1",
            Meta = new Meta
            {
                Tag = new List<Coding>
                {
                    new Coding(system: Constants.UrlTagOne, code: "true"),
                    new Coding(system: "https://my-url.com/CrIteria-two", code: dateTimeWithOffset)
                }
            }
        };

        Assert.Throws<ArgumentNullException>(() => detectedIssue.UnsafeExtensionOne());
    }
}