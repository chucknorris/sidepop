using bdddoc.core;
using developwithpassion.bdd.contexts;
using developwithpassion.bdd.mbunit;
using developwithpassion.bdd.mbunit.standard;
using developwithpassion.bdd.mbunit.standard.observations;

namespace sidepop.tests.integration
{
    using contexts;
    using Mail;

    public class MailMessagingSpecs
    {
        public abstract class concern_for_receiving_messages : observations_for_a_static_sut
        {
        }

        [Concern(typeof(SidePOPMailMessage))]
        public class when_creating_messages_with_different_allowable_fields_from_the_RFC : concern_for_receiving_messages
        {
            protected static object result;
			
            context c = () =>
                            {
			            
                            };

            because b = () => SidePOPMailMessage.CreateMailAddress(RFCValidEmailContext.email_address()); 
            //result = SidePOPMailMessage.CreateMailAddress;

            [Observation]
            public void should_allow_anything_with_the_correct_fields()
            {
                //SidePOPMailMessage.CreateMailAddress("dude%d%d@somewhere.com");
            }
        }
    }
}