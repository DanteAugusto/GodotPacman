using Godot;
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

public partial class GerenciadorCSV : Node
{
	public static GerenciadorCSV Instance { get; private set; }
	
	// SINGLETON
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			QueueFree();
		}

		InicializarArquivo("user://relatorio_pachand.csv");
	}
	
	private string _caminhoAbsoluto;
	private StreamWriter _writer;

	// Estrutura para leitura dos dados
	public class RegistroDados
	{
		public int NumeroPartida { get; set; }
        public float AdmFlexao { get; set; }
        public float VelocidadeMediaFlexao { get; set; }
        public float AdmExtensao { get; set; }
        public float VelocidadeMediaExtensao { get; set; }
        public float AdmDesvioRadial { get; set; }
        public float VelocidadeMediaDesvioRadial { get; set; }
        public float AdmDesvioUlnar { get; set; }
        public float VelocidadeMediaDesvioUlnar { get; set; }
        public int Score { get; set; }
        public float TempoSessao { get; set; }
	}

	public void InicializarArquivo(string caminhoGodot)
	{
		_caminhoAbsoluto = ProjectSettings.GlobalizePath(caminhoGodot);
		
		string diretorio = Path.GetDirectoryName(_caminhoAbsoluto);
		if (!Directory.Exists(diretorio))
		{
			Directory.CreateDirectory(diretorio);
		}

		_writer = new StreamWriter(_caminhoAbsoluto, append: true);

		// Se o arquivo for novo (tamanho 0), grava o cabeçalho estruturado
		if (new FileInfo(_caminhoAbsoluto).Length == 0)
        {
            _writer.WriteLine("numero_partida,adm_flexao,vel_media_flexao,adm_extensao,vel_media_extensao," +
                              "adm_desvio_radial,vel_media_desvio_radial,adm_desvio_ulnar,vel_media_desvio_ulnar," +
                              "score,tempo_sessao");
            _writer.Flush();
        }
	}

	public void SalvarLinha(RegistroDados dados)
    {
        if (_writer == null) return;

        // Monta a linha acessando as propriedades diretamente e garantindo o ponto decimal
        string linha = $"{dados.NumeroPartida}," +
                       $"{dados.AdmFlexao.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.VelocidadeMediaFlexao.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.AdmExtensao.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.VelocidadeMediaExtensao.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.AdmDesvioRadial.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.VelocidadeMediaDesvioRadial.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.AdmDesvioUlnar.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.VelocidadeMediaDesvioUlnar.ToString(CultureInfo.InvariantCulture)}," +
                       $"{dados.Score}," +
                       $"{dados.TempoSessao.ToString(CultureInfo.InvariantCulture)}";

        _writer.WriteLine(linha);
		_writer.Flush();
    }

	public List<RegistroDados> LerArquivo()
	{
		var lista = new List<RegistroDados>();

		if (!File.Exists(_caminhoAbsoluto)) return lista;

		// Fechamos temporariamente o writer para garantir a leitura limpa do arquivo
		_writer?.Close();

		using (var reader = new StreamReader(_caminhoAbsoluto))
		{
			if (!reader.EndOfStream) reader.ReadLine(); // Pula cabeçalho

			while (!reader.EndOfStream)
			{
				string linha = reader.ReadLine();
				if (string.IsNullOrWhiteSpace(linha)) continue;

				string[] colunas = linha.Split(',');
				if (colunas.Length >= 11)
				{
					try
                    {
						lista.Add(new RegistroDados
						{
							NumeroPartida = int.Parse(colunas[0]),
							AdmFlexao = float.Parse(colunas[1], CultureInfo.InvariantCulture),
							VelocidadeMediaFlexao = float.Parse(colunas[2], CultureInfo.InvariantCulture),
							AdmExtensao = float.Parse(colunas[3], CultureInfo.InvariantCulture),
							VelocidadeMediaExtensao = float.Parse(colunas[4], CultureInfo.InvariantCulture),
							AdmDesvioRadial = float.Parse(colunas[5], CultureInfo.InvariantCulture),
							VelocidadeMediaDesvioRadial = float.Parse(colunas[6], CultureInfo.InvariantCulture),
							AdmDesvioUlnar = float.Parse(colunas[7], CultureInfo.InvariantCulture),
							VelocidadeMediaDesvioUlnar = float.Parse(colunas[8], CultureInfo.InvariantCulture),
							Score = int.Parse(colunas[9]),
							TempoSessao = float.Parse(colunas[10], CultureInfo.InvariantCulture)
						});
					}
                    catch (FormatException)
                    {
                        GD.PrintErr($"Erro de conversão de dados na linha: {linha}");
                    }
				}
			}
		}

		// Reabre o writer para permitir novas gravações se necessário
		_writer = new StreamWriter(_caminhoAbsoluto, append: true);
		return lista;
	}

	public override void _ExitTree()
	{
		// Proteção essencial do ciclo de vida do Godot: fecha o arquivo ao sair do jogo
		if (_writer != null)
		{
			_writer.Close();
			_writer.Dispose();
		}
	}
}
