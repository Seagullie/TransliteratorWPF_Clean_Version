using System;

namespace TransliteratorWPF_Version.Services
{
    // TODO: Write the implementation
    public class StateOverlayService
    {
        private static StateOverlayService _instance;

        private StateOverlayService()
        {
        }

        public static StateOverlayService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new StateOverlayService();
            }
            return _instance;
        }
    }
}