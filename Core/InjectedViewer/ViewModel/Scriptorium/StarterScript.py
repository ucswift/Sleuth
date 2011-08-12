# Uncomment the following lines if you need to use the types they import.
#import clr
#clr.AddReference("PresentationCore")
#clr.AddReference("PresentationFramework")
#clr.AddReference("System.Windows.Forms")

from System.Collections import *
from System.Collections.Generic import *
from System.Diagnostics import *
#from System.Windows import *
#from System.Windows.Forms import *
#from System.Windows.Controls import *

if INPUT <> None :
	print "INPUT variable is " + INPUT.GetType().FullName
else :
	print "The INPUT variable has no value."

Debug.WriteLine("This text is sent to the Debug Output workspace, if it is open.")
