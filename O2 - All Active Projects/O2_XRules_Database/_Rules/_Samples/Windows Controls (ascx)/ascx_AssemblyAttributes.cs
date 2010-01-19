// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using O2.Kernel;
using O2.Kernel.Interfaces.O2Core;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.Windows;
using O2.DotNetWrappers.Network;
using O2.External.WinFormsUI.Forms;
using O2.Kernel.Interfaces.Views;
using O2.Views.ASCX;
using O2.Rules.OunceLabs.DataLayer_OunceV6;
using O2.Views.ASCX.MerlinWizard;
using O2.Views.ASCX.MerlinWizard.O2Wizard_ExtensionMethods;
//O2Tag_AddReferenceFile:merlin.dll
using Merlin;
using MerlinStepLibrary;
//O2Tag_AddReferenceFile:nunit.framework.dll
using NUnit.Framework; 
//O2File:C:\O2\O2 - All Active Projects\O2 - All Active Projects\O2Core\O2_Kernel\InterfacesBaseImpl\KReflection.cs
//O2File:C:\O2\O2 - All Active Projects\O2 - All Active Projects\O2Core\O2_Kernel\Interfaces\O2Core\IReflection.cs
//O2File: C:\O2\O2 - All Active Projects\O2 - All Active Projects\O2Core\O2_DotNetWrappers\Network\Web.cs

using O2.Kernel.InterfacesBaseImpl;

using O2.Script;
//O2File:extra.cs

namespace O2.Script
{	
    public class ascx_AssemblyAttributes : UserControl
    {    
    	
    	public TreeView tvWizards;
    	private static IO2Log log = PublicDI.log;
		private static IReflection reflection = new KReflection(); //PublicDI.reflection;

        public static void openAscxInNewWindow()
    	{
    		var executeWizards = (ascx_AssemblyAttributes)O2AscxGUI.openAscx(typeof(ascx_AssemblyAttributes), O2DockState.Float,"Execute Wizards");    		
    		var urlWithTargetFile = @"http://deploy.o2-ounceopen.com/DemoFiles/DotNet/HacmeBank_v2_WS.dll.zip";
    		var targetFile = Web.checkIfFileExistsAndDownloadIfNot("HacmeBank_v2_WS.dll", urlWithTargetFile);
    		if (File.Exists(targetFile))
    		{
    			log.debug("File exists: {0}", targetFile);
    			executeWizards.loadAssembly(targetFile);
    		}    		
    	}
    	 
		public ascx_AssemblyAttributes()
		{
		    InitializeComponent();	            
        }
    
        public void InitializeComponent()
        {
        	tvWizards = this.add_TreeView();
        	tvWizards.AllowDrop = true;
        	tvWizards.DragEnter += (sender, e) => Dnd.setEffect(e);
            tvWizards.DragDrop += (sender,e) => loadAssembly(Dnd.tryToGetFileOrDirectoryFromDroppedObject(e));
        	this.Width = 500;
        	this.Height = 500;
     	}   
     	
     	public void loadAssembly(string assemblyToLoad)
     	{
     		if (false == File.Exists(assemblyToLoad))
     		{
     			PublicDI.log.error("Could not load file: {0}", assemblyToLoad);
     			return;
     		} 	     		
     		var assembly = reflection.getAssembly(assemblyToLoad);
     		if (assembly != null)
     		{
     			var assemblyNode = tvWizards.add_Node("[Assembly]: " + assembly.GetName().Name);
     			
     			// map assembly attributes     			
     			foreach(var attribute in reflection.getAttributes(assembly))
     					tvWizards.add_Node(assemblyNode,"[attribute]: " + attribute.ToString(),Color.DarkRed);     				
     					
     			// map type attributes
     			foreach(var type in reflection.getTypes(assembly))
     			{
     				var typeAttributes = tvWizards.add_Node(assemblyNode,"[Type]: " + type.ToString());
					foreach(var attribute in reflection.getAttributes(type))
     					tvWizards.add_Node(typeAttributes, "[attribute]: " + attribute.ToString(),Color.DarkRed);     				
     					
     				// map method attributes     				
     				foreach(var method in reflection.getMethods(type))
     				{
     					var methodAttribute =  tvWizards.add_Node(typeAttributes,"[Method]: " + method.ToString());
     					foreach(var attribute in reflection.getAttributes(method))
     					{
     						tvWizards.add_Node(methodAttribute, "[attribute]: " + attribute.ToString(),Color.DarkRed);     				
     					}
     				}
     			}
     		}
     	}
    }
}
