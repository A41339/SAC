namespace FGA.Model
{
    public class Serie
    {
        public bool mostrar;
        public string nombre;
        public decimal total;
        public object[] valores;
        public string porcentual;

        public Serie(int pCantidad, string pNombre) {
            mostrar = false;
            nombre = pNombre;
            total = 0;
            valores = new object[pCantidad];

            for (int j = 0; j < pCantidad; j++)
                valores[j] = 0;
        }
    }
}