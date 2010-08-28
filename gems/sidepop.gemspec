version = File.read(File.expand_path("../VERSION",__FILE__)).strip

Gem::Specification.new do |s|
  s.platform    = Gem::Platform::RUBY
  s.name        = 'sidepop'
  s.version     = version
  s.files = Dir['lib/**/*']

  s.add_dependency('log4net','= 1.2.10')
  
  s.summary     = 'SidePOP - Email access for .NET. An easier way for your services and applications to receive email.'
  s.description = <<-EOF
SidePOP allows you to retrieve email very easily from a POP3 account. SidePOP is one DLL (with a dependency on log4net) and a simple configuration that allows you to enhance your applications by giving them the gift of receiving email. 
SidePOP has an easy configuration - it's the same settings you need to set up email on your phone or in a mail client to check your email. Then all you do is subscribe to the events and you are good. It can't get much harder than that. 
EOF
  
  s.authors            = ['Rob "FerventCoder" Reynolds','Tim Hibbard']
  s.email             = 'chucknorrisframework@googlegroups.com'
  s.homepage          = 'http://sidepop.googlecode.com'
  s.rubyforge_project = 'sidepop'
end