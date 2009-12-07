// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using O2.Kernel;
using O2.Kernel.Interfaces.O2Core;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.Windows;
using O2.Views.ASCX;
using O2.Kernel.Interfaces.Views;
using O2.External.SharpDevelop.Ascx;
using O2.Views.ASCX.classes;
using HTMLparserLibDotNet20.O2ExtraCode;
using O2.Views.ASCX.CoreControls;
using O2.Core.XRules.Classes;
using System.IO;
using O2.Kernel.CodeUtils;
using System.Threading;

namespace O2.Core.XRules.Ascx
{	    
  	public class ascx_SvnBrowser : UserControl
	{
		private static IO2Log log = PublicDI.log;
		
		public SplitContainer splitControl;		
		public GroupBox leftGroupBox;
		public GroupBox rightGroupBox;
		public TreeView tvDirectoriesAndFiles;
		public ascx_SourceCodeEditor sourceCodeEditor;

        static string svnBaseUrl = @"http://o2platform.googlecode.com/svn/trunk/";

		public ascx_SvnBrowser()
        {
        	log.info("in ascx_SvnBrowser constructor");
            InitializeComponent();
            openSvnUrl(svnBaseUrl);
        }
        
        public void InitializeComponent()
        {
        	splitControl = this.addSplitContainer(
            						false, 		//setOrientationToHorizontal
            						true,		// setDockStyleoFill
            						true);		// setBorderStyleTo3D)
            leftGroupBox = splitControl.Panel1.addGroupBox("Directories and Files");
            rightGroupBox = splitControl.Panel2.addGroupBox("FileContents");
            this.Width = 500;
            this.Height = 500;
            tvDirectoriesAndFiles = leftGroupBox.addTreeView();
            tvDirectoriesAndFiles.ImageList = ImagesLists.withFolderAndFile();
            tvDirectoriesAndFiles.NodeMouseDoubleClick += tvDirectoriesAndFiles_DoubleClick;
            tvDirectoriesAndFiles.ItemDrag += tvDirectoriesAndFiles_ItemDrag;
            sourceCodeEditor = rightGroupBox.addSourceCodeEditor();                        
        }

        void tvDirectoriesAndFiles_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode)
            {
                var treeNode = (TreeNode)e.Item;
                if (treeNode.Tag != null && treeNode.Tag is SvnMappedUrl)
                {
                    var svnMappedUrl = ((SvnMappedUrl)treeNode.Tag);
                    if (svnMappedUrl.IsFile)
                        DoDragDrop(svnMappedUrl.FullPath, DragDropEffects.Copy);
                }
            }
        }

        private void tvDirectoriesAndFiles_DoubleClick(object sender, EventArgs e)
        {        	
        	if (tvDirectoriesAndFiles.SelectedNode != null)
        	{
        		var svnMappedUrl = (SvnMappedUrl)tvDirectoriesAndFiles.SelectedNode.Tag;
        		log.info("on after select for: {0}", svnMappedUrl.Text);
        		
        		if (svnMappedUrl.IsFile)
	        		sourceCodeEditor.setDocumentContents(svnMappedUrl.getFileContents(), svnMappedUrl.VirtualPath);
    			else    		        		
        			openSvnUrl(svnMappedUrl.FullPath);        		
        	}
        }
        
        public void openSvnUrl(string urlToOpen)
        {
            tvDirectoriesAndFiles.invokeOnThread(
                () =>
                {
                    if (false == urlToOpen.StartsWith("http"))
                        urlToOpen = svnBaseUrl + urlToOpen;
                    tvDirectoriesAndFiles.clear();
                    var svnMappedUrls = SvnApi.getSvnMappedUrls(urlToOpen);
                    foreach (var svnMappedUrl in svnMappedUrls)
                    {
                        var newTreeNode =
                            tvDirectoriesAndFiles.addNode(
                                svnMappedUrl.Text,
                                (svnMappedUrl.IsFile) ? 1 : 0,
                                (svnMappedUrl.IsFile) ? Color.Blue : Color.Black,
                                (object)svnMappedUrl);
                    }
                });
        	
        }

        public static Thread openInFloatWindow()
        {
            return openInFloatWindow(svnBaseUrl);
        }

        public static Thread openInFloatWindow(string svnPath)
        {
            return O2Thread.mtaThread(
                () =>
                {
                    var windowTitle = "Svn Browser: " + svnPath;
                    O2Messages.openControlInGUISync(typeof(ascx_SvnBrowser), O2DockState.Float, windowTitle);
                    O2Messages.getAscx(windowTitle, guiControl =>
                                                        {
                                                            if (guiControl != null && guiControl is ascx_SvnBrowser)
                                                            {
                                                                var svnBrowser = (ascx_SvnBrowser)guiControl;
                                                                svnBrowser.openSvnUrl(svnPath);
                                                            }
                                                        });
                });
        }
	}		
}
