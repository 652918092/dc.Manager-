using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace dc.Haiyakj.Localization
{
    public static class AbpProjectNameLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(AbpProjectNameConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(AbpProjectNameLocalizationConfigurer).GetAssembly(),
                        "dc.Haiyakj.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
