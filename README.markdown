Project SidePOP - A simpler way to do automation interaction.

============
REQUIREMENTS
============
* .NET Framework 3.5 

======
DONATE
======
Donations Accepted - If you enjoy using this product or it has saved you time and money in some way, please consider making a donation. It helps keep to the product updated, pays for site hosting, etc. https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=10353308

=============
RELEASE NOTES
=============
=0.0.0.24=
* General fixes
* Removed the container dependency for logging

=0.0.0.15=
* Added more fault tolerance for network ups and downs

=0.0.0.13=
* Added an XMLConfigurator (fixing the name). 
* Changed Runner to EmailWatcher. 
* Added timeoutInMinutes to the configuration.

=0.0.0.11=
* Has a runner that you have to configure yourself - see BombaliServiceRunner (http://bombali.googlecode.com/svn/trunk/product/bombali/runners/BombaliServiceRunner.cs)

=======
CREDITS
=======
UppercuT - Automated Builds (automated build in 10 minutes or less?!) http://projectuppercut.org
CodeProject and .NET POP3 MIME Client - SidePOP is heavily based on this utility (much of the source code for actually hitting the service comes from this article) - http://www.codeproject.com/KB/IP/NetPopMimeClient.aspx