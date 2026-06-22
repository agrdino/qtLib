// using System.Threading.Tasks;
// using UI.Base;
// using UI.Popup.NotiPopup;
// using UI.Popup.YesNoPopup;
//
// namespace Helper
// {
//     public static class qtUiFlowExt
//     {
//         public static void ShowNotiPopup(string content)
//         {
//             qtUiFlow.Request<NotiPopupMediator>().BeforeUIShow((ui, logic, mediator) =>
//             {
//                 ui.txtContent.SetText(content);
//                 return UniTask.CompletedTask;
//             }).Move();
//         }
//
//         public static UniTask<YesNoPopupParamOutput> ShowYesNoPopup(string content)
//         {
//             YesNoPopupParamOutput decision =
//                 await qtUiFlow.Request<YesNoPopupMediator>().BeforeUIShow((ui, logic, mediator) =>
//                 {
//                     ui.txtContent.SetText(content);
//                     return UniTask.CompletedTask;
//                 }).Move<YesNoPopupParamOutput>();
//             return decision;
//         }
//     }
// }