using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Relatorios : Control
{
	// Referências dos Gráficos
	[Export] private GraficoLinha _graficoAdmFlexao;
	[Export] private GraficoLinha _graficoAdmExtensao;
	[Export] private GraficoLinha _graficoAdmDesvioUlnar;
	[Export] private GraficoLinha _graficoAdmDesvioRadial;
	[Export] private GraficoLinha _graficoVelocidadeFlexao;
	[Export] private GraficoLinha _graficoVelocidadeExtensao;
	[Export] private GraficoLinha _graficoVelocidadeDesvioUlnar;
	[Export] private GraficoLinha _graficoVelocidadeDesvioRadial;
	[Export] private GraficoLinha _graficoScore;
	[Export] private GraficoLinha _graficoTempo;
	
	// Referência das Categorias
	[Export] private VBoxContainer _graficosFlexao;
	[Export] private VBoxContainer _graficosExtensao;
	[Export] private VBoxContainer _graficosDesvioUlnar;
	[Export] private VBoxContainer _graficosDesvioRadial;
	[Export] private VBoxContainer _graficosScoreTempo;

	// Referências dos Botões
	[Export] private Button _btnFlexao;
	[Export] private Button _btnExtensao;
	[Export] private Button _btnDesvioUlnar;
	[Export] private Button _btnDesvioRadial;
	[Export] private Button _btnScoreTempo;

	public override void _Ready()
	{
		// 3. Conexão dos Sinais (Usando expressões lambda para direcionar qual gráfico abrir)
		_btnFlexao.Pressed += () => MostrarApenasGrafico(_graficosFlexao);
		_btnExtensao.Pressed += () => MostrarApenasGrafico(_graficosExtensao);
		_btnDesvioUlnar.Pressed += () => MostrarApenasGrafico(_graficosDesvioUlnar);
		_btnDesvioRadial.Pressed += () => MostrarApenasGrafico(_graficosDesvioRadial);
		_btnScoreTempo.Pressed += () => MostrarApenasGrafico(_graficosScoreTempo);

		// 4. Carrega os dados iniciais do CSV nos três gráficos
		CarregarDadosDoHistorico();

		// 5. Define o estado inicial da tela: mostra apenas o Score por padrão
		MostrarApenasGrafico(_graficosFlexao);
	}

	// A mágica acontece aqui: avalia cada gráfico individualmente
	private void MostrarApenasGrafico(VBoxContainer graficosAlvo)
	{
		// Uma comparação direta resulta em um booleano (true ou false)
		_graficosFlexao.Visible = (_graficosFlexao == graficosAlvo);
		_graficosExtensao.Visible = (_graficosExtensao == graficosAlvo);
		_graficosDesvioUlnar.Visible = (_graficosDesvioUlnar == graficosAlvo);
		_graficosDesvioRadial.Visible = (_graficosDesvioRadial == graficosAlvo);
		_graficosScoreTempo.Visible = (_graficosScoreTempo == graficosAlvo);
	}

	private void CarregarDadosDoHistorico()
	{
		List<GerenciadorCSV.RegistroDados> historico = GerenciadorCSV.Instance.LerArquivo();

		if (historico.Count >= 2)
		{
			_graficoAdmFlexao.CarregarDados(historico.Select(d => (float)d.AdmFlexao).ToList());
			_graficoAdmExtensao.CarregarDados(historico.Select(d => (float)d.AdmExtensao).ToList());
			_graficoAdmDesvioUlnar.CarregarDados(historico.Select(d => (float)d.AdmDesvioUlnar).ToList());
			_graficoAdmDesvioRadial.CarregarDados(historico.Select(d => (float)d.AdmDesvioRadial).ToList());
			_graficoVelocidadeFlexao.CarregarDados(historico.Select(d => (float)d.VelocidadeMediaFlexao).ToList());
			_graficoVelocidadeExtensao.CarregarDados(historico.Select(d => (float)d.VelocidadeMediaExtensao).ToList());
			_graficoVelocidadeDesvioUlnar.CarregarDados(historico.Select(d => (float)d.VelocidadeMediaDesvioUlnar).ToList());
			_graficoVelocidadeDesvioRadial.CarregarDados(historico.Select(d => (float)d.VelocidadeMediaDesvioRadial).ToList());
			_graficoScore.CarregarDados(historico.Select(d => (float)d.Score).ToList());
			_graficoTempo.CarregarDados(historico.Select(d => (float)d.TempoSessao).ToList());
		}
	}
}
