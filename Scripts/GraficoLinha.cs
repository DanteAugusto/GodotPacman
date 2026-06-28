using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

[GlobalClass] 
public partial class GraficoLinha : Control
{
	// --- TÍTULO ---
	[Export] public string Titulo { get; set; } = "Placeholder Titulo Grafico";
	[Export] public int TamanhoFonteTitulo { get; set; } = 18;
	
	[Export] public Color CorLinha { get; set; } = new Color("42a5f5"); // Azul
	[Export] public Color CorPonto { get; set; } = Colors.White;
	[Export] public Color CorEixos { get; set; } = new Color("888888"); // Cinza escuro
	[Export] public Color CorTexto { get; set; } = Colors.White;
	[Export] public Color CorGrid { get; set; } = new Color("2c2c2c");  // Linhas guias de fundo

	[Export] public float Margem { get; set; } = 60.0f; // Margem confortável para os eixos
	[Export] public float EspessuraLinha { get; set; } = 3.0f;
	[Export] public float RaioPonto { get; set; } = 5.0f;

	// 1. EIXO Y CONFIGURÁVEL: Defina os valores que deseja exibir direto pelo Inspetor do Godot
	[Export] public Godot.Collections.Array<float> MarcadoresEixoY { get; set; } = new() { 0f, 45f, 90f };

	private List<float> _dados = new List<float>();
	private List<Vector2> _posicoesPontos = new List<Vector2>(); // Cache de posições para cálculo de hover
	
	private int _pontoFocadoIndex = -1;
	private Vector2 _posicaoMouse = Vector2.Zero;
	
	public override void _Ready()
	{
		CarregarDados(new List<float> { 10f, 20f, 15f, 30f, 25f, 40f, 35f, 21f });
	}

	public void CarregarDados(List<float> dadosDaSessao)
	{
		_dados = dadosDaSessao;
		_pontoFocadoIndex = -1; // Reseta foco ao atualizar dados
		QueueRedraw(); 
	}

	// 2. DETECÇÃO DE MOUSE: Captura o movimento dentro da área do Gráfico
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_posicaoMouse = mouseMotion.Position;
			VerificarHoverPonto();
		}
	}

	private void VerificarHoverPonto()
	{
		int antigoFoco = _pontoFocadoIndex;
		_pontoFocadoIndex = -1;

		if (_dados == null || _dados.Count == 0 || _posicoesPontos.Count != _dados.Count) 
			return;

		// Varre os pontos calculados e verifica a distância matemática do mouse
		for (int i = 0; i < _posicoesPontos.Count; i++)
		{
			// Se o mouse estiver a menos de 12 pixels de distância do centro do ponto
			if (_posicaoMouse.DistanceTo(_posicoesPontos[i]) <= RaioPonto + 7.0f)
			{
				_pontoFocadoIndex = i;
				break;
			}
		}

		// Otimização importante: Só redesenha a tela se o ponto focado mudou
		if (_pontoFocadoIndex != antigoFoco)
		{
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		if (_dados == null || _dados.Count < 2) 
			return; 

		Vector2 tamanho = GetRect().Size;
		float larguraGrafico = tamanho.X - (Margem * 2);
		float alturaGrafico = tamanho.Y - (Margem * 2);
		Font fonte = GetThemeDefaultFont();
		
		// 1. DESENHO DO TÍTULO (Renderizado centralizado no topo)
		if (!string.IsNullOrEmpty(Titulo))
		{
			Vector2 tamanhoTextoTitulo = fonte.GetStringSize(Titulo, fontSize: TamanhoFonteTitulo);
			
			// Centraliza no X (tamanho.X / 2) e posiciona no Y um pouco acima da área do gráfico
			float tituloX = (tamanho.X / 2) - (tamanhoTextoTitulo.X / 2);
			float tituloY = (Margem / 2) + (tamanhoTextoTitulo.Y / 3); 
			
			Vector2 posTitulo = new Vector2(tituloX, tituloY);
			
			DrawString(fonte, posTitulo, Titulo, fontSize: TamanhoFonteTitulo, modulate: CorTexto);
		}

		// Define os limites máximos e mínimos do gráfico envelopando os dados reais + marcadores fixos
		float maxValor = _dados.Max();
		float minValor = _dados.Min();

		if (MarcadoresEixoY != null && MarcadoresEixoY.Count > 0)
		{
			maxValor = Mathf.Max(maxValor, MarcadoresEixoY.Max());
			minValor = Mathf.Min(minValor, MarcadoresEixoY.Min());
		}

		if (Mathf.IsEqualApprox(maxValor, minValor)) 
		{
			maxValor += 10f;
			minValor -= 10f;
		}

		
		int tamanhoFonte = GetThemeDefaultFontSize();

		// 3. DESENHO DO EIXO Y FIXO (GRID DE FUNDO E LABELS)
		if (MarcadoresEixoY != null)
		{
			foreach (float marcador in MarcadoresEixoY)
			{
				float porcentagemY = (marcador - minValor) / (maxValor - minValor);
				float y = (tamanho.Y - Margem) - (porcentagemY * alturaGrafico);

				// Linha horizontal cruzando o gráfico
				DrawLine(new Vector2(Margem, y), new Vector2(tamanho.X - Margem, y), CorGrid, 1.0f);

				// Texto correspondente ao marcador fixo
				string textoY = marcador.ToString("0.#", CultureInfo.InvariantCulture);
				Vector2 tamanhoTextoY = fonte.GetStringSize(textoY, fontSize: tamanhoFonte);
				Vector2 posTextoY = new Vector2(Margem - tamanhoTextoY.X - 10, y + (tamanhoTextoY.Y / 3));
				DrawString(fonte, posTextoY, textoY, fontSize: tamanhoFonte, modulate: CorTexto);

				// Tracinho no eixo principal
				DrawLine(new Vector2(Margem - 5, y), new Vector2(Margem, y), CorEixos, 1.5f);
			}
		}

		// 4. DESENHO DOS EIXOS PRINCIPAIS (X e Y)
		Vector2 origemEixos = new Vector2(Margem, tamanho.Y - Margem);
		Vector2 fimEixoX = new Vector2(tamanho.X - Margem + 15, tamanho.Y - Margem);
		Vector2 fimEixoY = new Vector2(Margem, Margem - 15);
		DrawLine(origemEixos, fimEixoX, CorEixos, 2.0f);
		DrawLine(origemEixos, fimEixoY, CorEixos, 2.0f);

		// 5. PROCESSAMENTO E FILTRAGEM DE POSIÇÕES (CACHE)
		_posicoesPontos.Clear();
		float espacamentoX = larguraGrafico / (_dados.Count - 1);
		for (int i = 0; i < _dados.Count; i++)
		{
			float x = Margem + (i * espacamentoX);
			float porcentagemY = (_dados[i] - minValor) / (maxValor - minValor);
			float y = (tamanho.Y - Margem) - (porcentagemY * alturaGrafico);
			_posicoesPontos.Add(new Vector2(x, y));
		}

		// 6. RENDERIZAÇÃO DO GRÁFICO (LINHAS E PONTOS)
		for (int i = 0; i < _posicoesPontos.Count; i++)
		{
			Vector2 pontoAtual = _posicoesPontos[i];

			if (i > 0)
			{
				DrawLine(_posicoesPontos[i - 1], pontoAtual, CorLinha, EspessuraLinha, antialiased: true);
			}

			// Se o ponto atual estiver sob o mouse, ele ganha destaque visual
			float raioAtual = (i == _pontoFocadoIndex) ? RaioPonto + 3.0f : RaioPonto;
			Color corPontoAtual = (i == _pontoFocadoIndex) ? CorLinha : CorPonto;
			DrawCircle(pontoAtual, raioAtual, corPontoAtual);

			// Texto estável do Eixo X (Número da Partida permanece fixo)
			string textoX = (i + 1).ToString();
			Vector2 tamanhoTextoX = fonte.GetStringSize(textoX, fontSize: tamanhoFonte);
			Vector2 posTextoX = new Vector2(pontoAtual.X - (tamanhoTextoX.X / 2), tamanho.Y - Margem + 22);
			DrawString(fonte, posTextoX, textoX, fontSize: tamanhoFonte, modulate: CorTexto);
			DrawLine(new Vector2(pontoAtual.X, tamanho.Y - Margem), new Vector2(pontoAtual.X, tamanho.Y - Margem + 5), CorEixos, 1.5f);
		}

		// 7. RENDERIZAÇÃO DO POPUP / TOOLTIP INTERATIVO
		if (_pontoFocadoIndex != -1 && _pontoFocadoIndex < _dados.Count)
		{
			float valor = _dados[_pontoFocadoIndex];
			Vector2 posPonto = _posicoesPontos[_pontoFocadoIndex];

			// Formatação do texto do balão
			string textoTooltip = $"Partida {_pontoFocadoIndex + 1}: {valor.ToString("0.0", CultureInfo.InvariantCulture)}";
			Vector2 tamanhoTexto = fonte.GetStringSize(textoTooltip, fontSize: tamanhoFonte);
			
			Vector2 padding = new Vector2(14, 8);
			Vector2 tamanhoCaixa = tamanhoTexto + padding;
			
			// Centraliza a caixinha do popup um pouco acima do ponto focado
			Vector2 posCaixa = posPonto + new Vector2(-tamanhoCaixa.X / 2, -tamanhoCaixa.Y - 15);

			// Alinhamento de segurança: impede que a caixinha saia das bordas do nó Control
			posCaixa.X = Mathf.Clamp(posCaixa.X, 0, tamanho.X - tamanhoCaixa.X);
			posCaixa.Y = Mathf.Max(posCaixa.Y, 0);

			// Desenha o fundo do balão (preto opaco/transparente)
			DrawRect(new Rect2(posCaixa, tamanhoCaixa), new Color(0.05f, 0.05f, 0.05f, 0.95f), filled: true);
			// Desenha a borda fina com a cor temática do próprio gráfico
			DrawRect(new Rect2(posCaixa, tamanhoCaixa), CorLinha, filled: false, width: 1.0f);

			// Posiciona e escreve o texto centralizado dentro do balão
			Vector2 posTexto = posCaixa + new Vector2(padding.X / 2, (tamanhoCaixa.Y / 2) + (tamanhoFonte / 3));
			DrawString(fonte, posTexto, textoTooltip, fontSize: tamanhoFonte, modulate: Colors.White);
		}
	}
}
