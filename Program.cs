using SistemaAcademico;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

public class Program
{
    private static UnidadeCurricular? _uc;
    private static readonly Dictionary<int, Estudante> _estudantesPorId = new();

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        CriarUnidadeCurricular();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("--- MENU ---");
            Console.WriteLine("1. Inscrever estudante");
            Console.WriteLine("2. Registar avaliação");
            Console.WriteLine("3. Emitir pauta");
            Console.WriteLine("4. Listar estudantes inscritos");
            Console.WriteLine("0. Sair");

            var op = Input.ReadOption("Opção: ", 0, 1, 2, 3, 4);

            try
            {
                switch (op)
                {
                    case 1: InscreverEstudante(); break;
                    case 2: RegistarAvaliacao(); break;
                    case 3: EmitirPauta(); break;
                    case 4: ListarInscritos(); break;
                    case 0: return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }

    private static void CriarUnidadeCurricular()
    {
        Console.WriteLine("-- Configuração Inicial --");
        var codigo = Input.ReadNonEmpty("Código da Unidade Curricular: ");
        var nomeUC = Input.ReadNonEmpty("Nome da Unidade Curricular: ");

        Console.WriteLine("Docente responsável:");
        var docenteId = Input.ReadInt("Id do docente: ");
        var docenteNome = Input.ReadNonEmpty("Nome do docente: ");
        var dept = Input.ReadNonEmpty("Departamento: ");
        var tipo = Input.ReadOption("Tipo (1-Titular, 2-Assistente): ", 1, 2);

        Docente resp = tipo == 1
            ? new Titular(docenteId, docenteNome, dept)
            : new Assistente(docenteId, docenteNome, dept);

        _uc = new UnidadeCurricular(codigo, nomeUC, resp);

        Console.WriteLine("Unidade Curricular criada com sucesso.");
    }

    private static void InscreverEstudante()
    {
        if (_uc is null) throw new InvalidOperationException("Unidade Curricular não inicializada.");

        Console.WriteLine("--- Inscrever Estudante ---");
        var id = Input.ReadInt("Id do estudante: ");

        if (_estudantesPorId.ContainsKey(id))
        {
            Console.WriteLine("Já existe um estudante com esse Id. A inscrever na Unidade Currricular (se ainda não estiver).");
            _uc.InscreverEstudante(_estudantesPorId[id]);
            return;
        }

        var nome = Input.ReadNonEmpty("Nome do estudante: ");
        var numero = Input.ReadNonEmpty("Número/Matrícula: ");

        var e = new Estudante(id, nome, numero);
        _estudantesPorId[id] = e;
        _uc.InscreverEstudante(e);

        Console.WriteLine("Estudante inscrito com sucesso.");
    }

    private static Estudante EscolherEstudanteInscrito()
    {
        if (_uc is null) throw new InvalidOperationException("UC não inicializada.");

        if (_uc.Inscritos.Count == 0)
            throw new InvalidOperationException("Não há estudantes inscritos.");

        Console.WriteLine("--- Estudantes Inscritos ---");
        foreach (var e in _uc.Inscritos.OrderBy(x => x.Nome))
            Console.WriteLine($"Id={e.Id} | {e.Numero} - {e.Nome}");

        var id = Input.ReadInt("Indica o Id do estudante: ");

        var estudante = _uc.Inscritos.FirstOrDefault(x => x.Id == id);
        if (estudante is null) throw new InvalidOperationException("Esse Id não está inscrito nesta UC.");

        return estudante;
    }

    private static void RegistarAvaliacao()
    {
        if (_uc is null) throw new InvalidOperationException("UC não inicializada.");

        Console.WriteLine("--- Registar Avaliação ---");
        var e = EscolherEstudanteInscrito();

        Console.WriteLine("Tipo de avaliação:");
        Console.WriteLine("1. Teste");
        Console.WriteLine("2. Projecto");
        Console.WriteLine("3. Exame Final");

        var tipo = Input.ReadOption("Opção: ", 1, 2, 3);

        var nota = Input.ReadDouble("Nota (0..20): ", 0, 20);
        var peso = Input.ReadDouble("Peso (0..1): ", 0.0000001, 1);

        Avaliacao a = tipo switch
        {
            1 => new Teste(nota, peso),
            2 => new Projecto(nota, peso),
            3 => new ExameFinal(nota, peso),
            _ => throw new InvalidOperationException("Tipo inválido.")
        };

        _uc.RegistarAvaliacao(e, a);
        Console.WriteLine($"Avaliação registada para {e.Nome}. Parcela: {a.CalcularNotaFinal():0.00}");
    }

    private static void EmitirPauta()
    {
        if (_uc is null) throw new InvalidOperationException("UC não inicializada.");

        Console.WriteLine();
        Console.WriteLine(_uc.EmitirPautaEmTexto());
    }

    private static void ListarInscritos()
    {
        if (_uc is null) throw new InvalidOperationException("UC não inicializada.");

        if (_uc.Inscritos.Count == 0)
        {
            Console.WriteLine("Sem estudantes inscritos.");
            return;
        }

        Console.WriteLine("--- Inscritos ---");
        foreach (var e in _uc.Inscritos.OrderBy(x => x.Nome))
            Console.WriteLine($"{e.Numero} - {e.Nome} (Id={e.Id})");
    }
}

