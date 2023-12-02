using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Useme.Clients.Wpf.Persistence.Jobs;
using VRT.FreelanceJobs.Wpf.Options;

namespace VRT.FreelanceJobs.Wpf.Services.Upwork;

internal static class StringExtensions
{
    public static IEnumerable<Job> ToUpworkJobs(this string? htmlString)
    {
        if (string.IsNullOrWhiteSpace(htmlString))
        {
            yield break;
        }
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlString);
        var jobsDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='jobs']");
        var jobs = jobsDiv.SelectNodes("div[@class='job']");
        foreach (var jobDiv in jobs)
        {
            var jobLink = jobDiv.SelectSingleNode(".//a[contains(@class,'job__title-link')]");
            var footer = jobDiv.SelectSingleNode(".//div[@class='job__footer']/div");
            var job = new Job()
            {
                Id = jobLink.GetId(),
                SourceName = UpworkOptions.SourceName,
                JobTitle = jobLink.TrimInnerText()!,
                FullOfferDetailsUrl = $"{UpworkOptions.SourceName}{jobLink.Attributes["href"]?.Value?.Trim()}",
                OffersCount = jobDiv.SelectSingleNode(".//div[@class[contains(.,'job__header-details--offers')]]/span[2]").TrimInnerText(),
                OfferDueDate = jobDiv.SelectSingleNode(".//div[@class[contains(.,'job__header-details--date')]]/span[2]").TrimInnerText().ToPolishDate(),
                ContentShort = jobDiv.SelectSingleNode(".//div[@class[contains(.,'job__content')]]/p").TrimInnerText(),
                Category = footer?.SelectSingleNode("./div[@class='job__category']/a/p").TrimInnerText(),
                Budget = footer?.SelectSingleNode("./div[@class='job__budget']/span[2]").TrimInnerText(),
                Skills = (footer
                    ?.SelectNodes("./div[@class='job__skills']/div[2]")
                    ?.Where(e => e is not null)
                    ?.Select(e => e.TrimInnerText() ?? "")
                    ?.Where(e => string.IsNullOrWhiteSpace(e) == false)
                    ?.ToArray()) ?? Array.Empty<string>()
            };
            if (string.IsNullOrWhiteSpace(job.Id) is false && string.IsNullOrWhiteSpace(job.OfferDueDate) is false)
            {
                yield return job;
            }
        }
    }
    private static string? ToPolishDate(this string? date)
    {
        var dateParts = date?.Split('.');
        return dateParts switch
        {
            [var day, var month, var year] => $"20{year}-{month}-{day}",
            _ => null //invalid date or alredy closed
        };
    }
    private static string? TrimInnerText(this HtmlNode node)
    {
        return node?.InnerText?.Trim('\r', '\n', '\t', ' ', '"');
    }
    private static string GetId(this HtmlNode node)
    {
        var attr = node.Attributes["href"].Value;
        var match = Regex.Match(attr ?? "", @"^.*?,(?<id>\d+)[/\s\r\n]*$", RegexOptions.NonBacktracking);
        return match?.Groups["id"].Value ?? "";
    }
}
