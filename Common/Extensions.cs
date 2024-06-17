using System.Globalization;
using Hl7.Fhir.Model;

namespace Common;

public static class Extensions
{
	/// <summary>
	/// Example 1
	/// Based on the method signature, you could assume, returns a nullable string if unexpected data
	/// However, the offset is dynamic based on the defined time zone on the user's local computer.
	/// Difficult to read, I assumed UnsafeExtensionOne would throw. 
	/// </summary>
	/// <param name="detectedIssue"></param>
	/// <returns>Actually returns formatted date and time | formatted min date and time | null</returns>
	public static string? UnsafeExtensionOne(this DetectedIssue detectedIssue)
	{
		// Check if offline mode is true, and if so set it
		//  Risk: not escaping case sensitivity.
		var isCriteriaOne = detectedIssue.Meta.Tag.FirstOrDefault(x => x.System == Constants.UrlTagOne)?.Code == "true";
		string? formattedDateTime = null;
		var offset = DateTimeOffset.Now.Offset;
		// if the alert was submitted offline, get the sync time
		if (isCriteriaOne == true)
		{
			//  Risk: not escaping case sensitivity.
			var coding = isCriteriaOne ? detectedIssue.Meta.Tag.Find(x => x.System == Constants.UrlTagTwo) : null;
			//var taskSyncDateTime = DateTimeOffset.Parse(taskOfflineModeExtension?.Code);
			DateTimeOffset dateTimeOffset;
			try
			{
				dateTimeOffset = DateTimeOffset.Parse(coding?.Code);
			}
			catch (FormatException)
			{
				if (!DateTimeOffset.TryParseExact(coding?.Code, "MM/dd/yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset))
				{
					Console.WriteLine("the date time provided is invalid");
				}
			}
			
			var systemOffset = dateTimeOffset.ToOffset(offset);
			formattedDateTime = systemOffset.DateTime.ToString("dd MMM yyyy HH:mm");
		}

		return formattedDateTime;
	}
	
	/// <summary>
	/// More defensive version of the above and includes offset calculation in the context of the Patient or other resource.
	/// Does not format the date time as that is a responsibility of the consuming application.
	/// </summary>
	/// <param name="detectedIssue"></param>
	/// <param name="timeZoneId">Time zone id with default. Related to the Patient or other resource.</param>
	/// <returns></returns>
	public static DateTimeOffset? ImprovedExtensionTwo(this DetectedIssue detectedIssue, string timeZoneId = Constants.DefaultTimeZoneId)
	{
		//  If the DetectedIssue doesn't have any tags (both Meta & Tag are nullable), return early.
		if (detectedIssue?.Meta?.Tag is null || !detectedIssue.Meta.Tag.Any()) return default;
		
		//  Verify if DetectedIssue has a custom tag of system == /criteria-one and the verify if the string value is true or false with a fallback of false.
		var isCriteriaOne = detectedIssue.Meta.Tag
			.FirstOrDefault(p => p.System.Equals(Constants.UrlTagOne, StringComparison.OrdinalIgnoreCase))
			?.Code.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
		
		//  Return early.
		if (isCriteriaOne == false) return default;
		
		var coding = detectedIssue.Meta.Tag
			.FirstOrDefault(p => p.System.Equals(Constants.UrlTagTwo, StringComparison.OrdinalIgnoreCase));
		
		//  Return early if invalid datetime with offset.
		if (!DateTimeOffset.TryParse(coding?.Code, out var dateTimeOffset)) return default;
		
		var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
		var baseUtcOffset = timeZoneInfo.BaseUtcOffset;

		var taskSyncDateTime = new DateTimeOffset(dateTimeOffset.DateTime.AddHours(baseUtcOffset.Hours), baseUtcOffset);
		return taskSyncDateTime;
	}

	/// <summary>
	/// Optimistically attempts to parse string datetime value.
	///
	/// Date and time is not easy. Due to the complexity, risk and global variety of date and time formats, I think its best
	/// to resolve this responsibility as the value is entered as input. During transit via dto, the value can then be the appropriate type: Date, DateTime, Time, DateTimeOffset, ...
	/// As the value is consumed by the client/s, then it can be formatted. 
	/// </summary>
	/// <param name="dateTimeString"></param>
	/// <param name="knownFormats">list of known formats to attempt to parse</param>
	/// <param name="cultureId"></param>
	/// <returns></returns>
	public static DateTimeOffset? ToDateTimeOffsetFromString(this string dateTimeString, string[] knownFormats, string cultureId = "en-US")
	{
		var cultureInfo = new CultureInfo(cultureId, false);

		//  Attempt to parse with known culture and format/s.
		if (!DateTimeOffset.TryParseExact(
			    dateTimeString,
			    knownFormats,
			    cultureInfo,
			    DateTimeStyles.None,
			    out var dateTimeWithOffsetValue))
		{
			//  Fallback, attempt to parse if format is standard.
			return !DateTimeOffset.TryParse(dateTimeString, out var dateTimeOffset)
				? default 
				: dateTimeOffset;
		}

		return dateTimeWithOffsetValue;
	}
}
