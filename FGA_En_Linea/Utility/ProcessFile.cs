using System;

namespace FGA.Utility
{
    public class ProcessFile
    {
        public static void ProcesarCierre(string id, DateTime periodo)
        {
            try
            {
                FGA_En_Linea.SPService.SPClient sp = new FGA_En_Linea.SPService.SPClient();
                sp.FGA_Cierre_Mensual(id, periodo);
                sp.FGA_Generar_Estructura_Fondeo(id, periodo);
            }
            catch (Exception)
            {
            }
        }   
    }
}