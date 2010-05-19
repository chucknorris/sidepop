using bdddoc.core;
using developwithpassion.bdd.contexts;
using developwithpassion.bdd.mbunit;
using developwithpassion.bdd.mbunit.standard;
using developwithpassion.bdd.mbunit.standard.observations;

namespace sidepop.tests.Mime
{
    using System.Net.Mime;

    public class ContentTypeSpecs
    {
        public abstract class concern_for_content_type : observations_for_a_static_sut
        {
        }

        [Concern(typeof(ContentType))]
        public class when_content_type_is_constructed : concern_for_content_type
        {
            protected static object result;
			
            context c = () =>
                            {
			            
                            };
        
            because b = () => { };

            [Observation]
            public void should_be_able_to_pass_text_plain_and_charset_utf_8_in_quotes()
            {
                ContentType ct = new ContentType("text/plain; charset=\"UTF-8\"");
            }

            [Observation]
            public void should_be_able_to_pass_text_plain_and_charset_utf_8_without_quotes()
            {
                ContentType ct = new ContentType("text/plain; charset=UTF-8");
            }
        }
    }
}