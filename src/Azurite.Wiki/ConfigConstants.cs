namespace Azurite.Wiki
{
    internal static class ConfigConstants
    {
        internal const string baseUrl = "https://azurlane.koumakan.jp/";

        internal const string ShipList = @"
            {
                'ships':
                {
                    '_xpath': '//div[@class=\'mw-parser-output\']//tr',
                    'id': './td[1]/@data-sort-value',
                    'name': './td[2]/a/@title',
                    'rarity': './td[3]/text()',
                    'type': './td[4]/a/text()',
                    'subtype': './td[contains(@style, \'background-\')][last()]/a[1]/text()',
                    'faction': './td[not(contains(@style, \'background-\'))][3]/a[1]/text()',
                }
            }";

        internal const string ShipDetails = @"{
                'header': '//div[@class=\'mw-parser-output\']//div[@class=\'azl_box_head\']',
                'id': '//th[contains(text(),\'ID\')][1]/following-sibling::td/text()',
                'name': '//meta[@property=\'og:title\'][1]/@content',
                'name_cn': '//div/span[@lang=\'zh\']/text()',
                'name_jp': '//div/span[@lang=\'ja\']/text()',
                'name_kr': '//div/span[@lang=\'ko\']/text()',
                'build_time_basic': '//a[contains(@href, \'Construction\')][not(contains(@href, \'Building\'))]/text()',
                'build_time': '//th[contains(text(), \'Construction Time\')][1]/following-sibling::td/a[1]/text()',
                'type_main': '//th[contains(text(), \'Classification\')][1]/following-sibling::td/a[contains(@title, \'Category\')][1]/text()',
                'type_sub': '//th[contains(text(), \'Classification\')][1]/following-sibling::td/a[contains(@title, \'Category\')][2]/text()',
                'class': '//th[contains(text(), \'Class\') and not(contains(text(), \'Classification\'))]/following-sibling::td/a[1]/text()',
                'stars': '//th[contains(text(), \'Rarity\')]/following-sibling::td/text()',
                'rarity': '//th[contains(text(), \'Rarity\')]/following-sibling::td/a[1]/@title',
                'faction_name': '//th[contains(text(), \'Nationality\')]/following-sibling::td/a[1]/@title',
                'title_full': '//div/span[@lang=\'zh\']/ancestor::div[1]/text()[1]'
            }";
    }
}