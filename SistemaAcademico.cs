using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SistemaAcademico
{
    public interface IAvaliavel
    {
        double CalcularNotaFinal();
    }

    public abstract class Pessoa
    {
        public int Id { get; }
        public string Nome { get; }

        protected Pessoa(int id, string nome)
        {
            Id = id;
            Nome = nome;
        }
    }

    public abstract class Docente : Pessoa
    {
        public string Departamento { get; }

        protected Docente(int id, string nome, string departamento) : base(id, nome)
        {
            Departamento = departamento;
        }
    }

    public class Titular : Docente
    {
        public Titular(int id, string nome, string departamento)
            : base(id, nome, departamento) { }
    }

    public class Assistente : Docente
    {
        public Assistente(int id, string nome, string departamento)
            : base(id, nome, departamento) { }
    }

    public class Estudante : Pessoa
    {
        public string Numero { get; }

        public Estudante(int id, string nome, string numero) : base(id, nome)
        {
            Numero = numero;
        }
    }

    public abstract class Avaliacao : IAvaliavel
    {
        public string Nome { get; }
        public double Nota { get; protected set; } 

        protected Avaliacao(string nome, double nota)
        {
            Nome = nome;
            DefinirNota(nota);
        }

        public void DefinirNota(double nota)
        {
            if (nota < 0 || nota > 20)
                throw new ArgumentOutOfRangeException(nameof(nota), "Nota deve estar entre 0 e 20.");
            Nota = nota;
        }

        public abstract double CalcularNotaFinal();
    }

    public class Teste : Avaliacao
    {
        public double Peso { get; }
        public Teste(double nota, double peso = 0.30) : base("Teste", nota)
        {
            if (peso <= 0 || peso > 1) throw new ArgumentOutOfRangeException(nameof(peso));
            Peso = peso;
        }
        public override double CalcularNotaFinal() => Nota * Peso;
    }

    public class Projecto : Avaliacao
    {
        public double Peso { get; }
        public Projecto(double nota, double peso = 0.30) : base("Projecto", nota)
        {
            if (peso <= 0 || peso > 1) throw new ArgumentOutOfRangeException(nameof(peso));
            Peso = peso;
        }
        public override double CalcularNotaFinal() => Nota * Peso;
    }

    public class ExameFinal : Avaliacao
    {
        public double Peso { get; }
        public ExameFinal(double nota, double peso = 0.40) : base("Exame Final", nota)
        {
            if (peso <= 0 || peso > 1) throw new ArgumentOutOfRangeException(nameof(peso));
            Peso = peso;
        }
        public override double CalcularNotaFinal() => Nota * Peso;
    }

    public class ResultadoPauta
    {
        public Estudante Estudante { get; }
        public double NotaFinal { get; }

        public ResultadoPauta(Estudante estudante, double notaFinal)
        {
            Estudante = estudante;
            NotaFinal = notaFinal;
        }

        public override string ToString()
            => $"{Estudante.Numero} - {Estudante.Nome} : {NotaFinal:0.00}";
    }

    public class UnidadeCurricular
    {
        public string Codigo { get; }
        public string Nome { get; }
        public Docente Responsavel { get; }

        private readonly List<Estudante> _inscritos = new();
        public IReadOnlyList<Estudante> Inscritos => _inscritos.AsReadOnly();

        
        private readonly List<Avaliacao> _avaliacoes = new();
        public IReadOnlyList<Avaliacao> Avaliacoes => _avaliacoes.AsReadOnly();

        private readonly Dictionary<int, List<Avaliacao>> _avaliacoesPorEstudante = new();

        public UnidadeCurricular(string codigo, string nome, Docente responsavel)
        {
            Codigo = codigo;
            Nome = nome;
            Responsavel = responsavel;
        }

        public void InscreverEstudante(Estudante e)
        {
            if (_inscritos.Any(x => x.Id == e.Id)) return;
            _inscritos.Add(e);
            _avaliacoesPorEstudante[e.Id] = new List<Avaliacao>();
        }

        public void RegistarAvaliacao(Estudante e, Avaliacao avaliacao)
        {
            if (!_avaliacoesPorEstudante.ContainsKey(e.Id))
                throw new InvalidOperationException("Estudante não está inscrito na unidade curricular.");

            _avaliacoesPorEstudante[e.Id].Add(avaliacao);
            _avaliacoes.Add(avaliacao);
        }

        public double CalcularNotaFinalDoEstudante(Estudante e)
        {
            if (!_avaliacoesPorEstudante.TryGetValue(e.Id, out var lista))
                throw new InvalidOperationException("Estudante não está inscrito na unidade curricular.");

            var soma = lista.Sum(a => a.CalcularNotaFinal());
            return Math.Max(0, Math.Min(20, soma));
        }

        public List<ResultadoPauta> EmitirPauta()
        {
            return _inscritos
                .Select(e => new ResultadoPauta(e, CalcularNotaFinalDoEstudante(e)))
                .OrderByDescending(r => r.NotaFinal)
                .ThenBy(r => r.Estudante.Nome)
                .ToList();
        }

        public string EmitirPautaEmTexto()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Pauta - {Codigo} {Nome}");
            sb.AppendLine($"Responsável: {Responsavel.Nome} ({Responsavel.GetType().Name})");
            sb.AppendLine(new string('-', 45));

            foreach (var r in EmitirPauta())
                sb.AppendLine(r.ToString());

            return sb.ToString();
        }
    }

    internal static class Input
    {
        public static string ReadNonEmpty(string label)
        {
            while (true)
            {
                Console.Write(label);
                var s = (Console.ReadLine() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(s)) return s;
                Console.WriteLine("Valor inválido. Tenta novamente.");
            }
        }

        public static int ReadInt(string label)
        {
            while (true)
            {
                Console.Write(label);
                if (int.TryParse(Console.ReadLine(), out var v)) return v;
                Console.WriteLine("Número inteiro inválido.");
            }
        }

        public static double ReadDouble(string label, double min, double max)
        {
            while (true)
            {
                Console.Write(label);
                if (double.TryParse(Console.ReadLine(), out var v) && v >= min && v <= max) return v;
                Console.WriteLine($"Valor inválido. Introduz um número entre {min} e {max}.");
            }
        }

        public static int ReadOption(string label, params int[] allowed)
        {
            while (true)
            {
                Console.Write(label);
                if (int.TryParse(Console.ReadLine(), out var v) && allowed.Contains(v)) return v;
                Console.WriteLine("Opção inválida.");
            }
        }
    }
}
