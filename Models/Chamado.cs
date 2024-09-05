namespace ServiceNowWeb.Models
{
    public class Chamado
    {
        public DateTime DataCriacao { get; set; }
        public string Host { get; set; }
        public string DescricaoResumida { get; set; }
    }

    //public class HostPrincipal
    //{
    //    public string? Nome { get; set; }

    //    public List<HostSecundario> ListaHostsSecundarios { get; set; }
    //}

    //public class HostSecundario
    //{
    //    public string Nome { get; set; }
    //    public int QtdeOcorrecias { get; set; }
    //}
}