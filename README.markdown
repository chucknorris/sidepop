Project SidePOP - A simpler way to do automation interaction.

=======
LICENSE
=======
Apache 2.0 - see docs\legal (just LEGAL in the zip folder)

=========
IMPORTANT
=========
NOTE: If you are looking at the source - please run build.bat before opening the solution. It creates the SolutionVersion.cs file that is necessary for a successful build.

====
INFO
====
SidePOP allows you to retrieve email very easily from a POP3 account. SidePOP is one DLL (with a dependency on log4net) and a simple configuration that allows you to enhance your applications by giving them the gift of receiving email. 

SidePOP has an easy configuration - it's the same settings you need to set up email on your phone or in a mail client to check your email. Then all you do is subscribe to the events and you are good. It can't get much harder than that. 

WARNING: SidePOP will delete your email when it checks it. That’s how it can be sure it’s only dealing with new messages every time. Do not test on an account you care about. You’ve been warned. 

NOTE: This is in alpha so I expect possible bugs. Please make sure you register bugs so they can be fixed: http://code.google.com/p/sidepop/issues/list 

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