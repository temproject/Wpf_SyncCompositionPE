#define TF_TEST

using LoadingWindow;
using LoadingWindow.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.References;
using Wpf_SyncCompositionPE;
using Wpf_SyncCompositionPE.Model;
using Wpf_SyncCompositionPE.ViewModel;
using System.Windows;
namespace ConsoleApplication
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ServerConnection sc = ConnectToDocs();

            Guid PM_ref_Guid = new Guid("86ef25d4-f431-4607-be15-f52b551022ff");


            //MacroContext mc = new MacroContext(sc);


            ReferenceObject ro = ReferenceCatalog.FindReference(PM_ref_Guid).CreateReference().Find(231204);

            Wpf_SyncCompositionPE.Model.SyncCompositionPE syncCompositionPE = new Wpf_SyncCompositionPE.Model.SyncCompositionPE();

            syncCompositionPE.StartRefObject = ro;

            if (HelperMethod.IsListNullOrEmpty(syncCompositionPE.DetailingProjects))
            {
                DisplayMessage.ShowError("Синхронизация состава работа", "Ошибка, выбранный элемент проекта не имеет детализаций!");
                return;
            }

            var view = new MainWindow();

            view.viewModel.syncCompositionPE = syncCompositionPE;

            view.ShowDialog();

            //dynamic macro = new Cancelaria.Macro(mc);
            //macro.Test();
            //return;
            //var project = References.ProjectManagementReference.Find(346161); //План ОПК
            //var dialog = new Report.Views.Report_View(project);
            //dialog.ShowDialog();
            Console.ReadKey();
        }
        static ServerConnection ConnectToDocs()
        {
#if TF_TEST
            //ServerGateway.Connect("administrator", new MD5HashString("saTflexTest1"), "TF-test");
            //ServerGateway.Connect("UNKNOWN\\14_4_0_100");
            ServerGateway.Connect("TFLEX");
            //ServerGateway.Connect("TF-TEST\\TF_TEST15");

#else

            ServerGateway.Connect("TFLEX");
            //ServerGateway.Connect("administrator", new MD5HashString("MHMr2QqQae"), "TFLEX");
#endif
            if (!ServerGateway.Connect(false))
            {
                Console.WriteLine("Не удалось подключиться к серверу.");
                return null;
            }
            //else { SaveToFile("Подключение к серверу, успешно."); }
#if !TF_TEST
            TFlex.DOCs.Model.Plugins.AssemblyLoader.LoadAssembly("TFlex.DOCs.ProjectManagement.dll");
            TFlex.DOCs.Model.Plugins.AssemblyLoader.LoadAssembly("TFlex.DOCs.UI.Common.dll");

            TFlex.DOCs.Model.Plugins.AssemblyLoader.LoadAssembly("TFlex.DOCs.UI.Objects.dll");
            TFlex.DOCs.Model.Plugins.AssemblyLoader.LoadAssembly("TFlex.DOCs.UI.Types.dll");
            TFlex.DOCs.Model.Plugins.AssemblyLoader.LoadAssembly("TFlex.DOCs.UI.Mail.dll");
#endif
            //ReferenceCatalog.RegisterSpecialReference(ProjectManagement);
            return ServerGateway.Connection;
        }
    }


}
