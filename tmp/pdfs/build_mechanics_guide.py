from pathlib import Path
from html import escape
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor, Color, white
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import Paragraph
from reportlab.lib.styles import ParagraphStyle

ROOT = Path(r'G:\Unity projects\TCC-Game')
OUT = ROOT / 'output/pdf/Guia_das_mecanicas_TCC_Game.pdf'
OUT.parent.mkdir(parents=True, exist_ok=True)
for name, file in [('Body','segoeui.ttf'),('Bold','segoeuib.ttf'),('Light','segoeuil.ttf'),('Mono','consola.ttf')]:
    pdfmetrics.registerFont(TTFont(name, str(Path('C:/Windows/Fonts') / file)))
pdfmetrics.registerFontFamily('Body', normal='Body', bold='Bold', italic='Body', boldItalic='Bold')
INK=HexColor('#18323D'); TEAL=HexColor('#137D80'); GOLD=HexColor('#B47535'); MUTED=HexColor('#536970'); PALE=HexColor('#EDF5F4'); RULE=HexColor('#D7E2E3')
W,H=595.276,841.89
c=canvas.Canvas(str(OUT),pagesize=(W,H))
c.setTitle('Mecânicas do TCC-Game | Guia de funcionamento e implementação')
c.setAuthor('Lain | Análise do projeto TCC-Game')
c.setSubject('24 mecânicas e sistemas explicados a partir dos scripts C# e das cenas Unity.')
PAGE=0
def para(text,x,y,width,size=10.3,leading=14,color=INK,font='Body'):
    p=Paragraph(text,ParagraphStyle('p',fontName=font,fontSize=size,leading=leading,textColor=color,spaceAfter=0))
    _,height=p.wrap(width,900)
    p.drawOn(c,x,y-height)
    return y-height
def line(text,x,y,size=10,font='Body',color=INK):
    c.setFillColor(color);c.setFont(font,size);c.drawString(x,y,text)
def page(title,kicker):
    global PAGE
    PAGE+=1
    c.setFillColor(TEAL);c.rect(0,H-9,W,9,fill=1,stroke=0)
    line('TCC-GAME  /  GUIA DE MECÂNICAS',44,808,8,'Bold',TEAL)
    line(kicker.upper(),44,775,8.5,'Bold',GOLD)
    y=para(title,44,762,W-88,23,28,font='Bold')
    c.setStrokeColor(RULE);c.line(44,44,W-44,44)
    line('Análise do código e das cenas | 10 nov. 2026',44,28,8,color=MUTED)
    c.setFont('Bold',9);c.drawRightString(W-44,28,f'{PAGE:02d}')
    c.bookmarkPage(f'p{PAGE}')
    c.addOutlineEntry(title,f'p{PAGE}',0,False)
    return y-22
def end(): c.showPage()
def block(label,text,y):
    y=para(label.upper(),44,y,W-88,8.5,12,TEAL,'Bold')-5
    return para(text,44,y,W-88,10.5,15)-16
def code(lines,y):
    height=17+len(lines)*12
    c.setFillColor(PALE);c.roundRect(44,y-height,W-88,height,5,fill=1,stroke=0)
    for i,t in enumerate(lines):
        assert pdfmetrics.stringWidth(t,'Mono',9.2)<W-112,t
        line(t,56,y-15-i*12,9.2,'Mono')
    return y-height-9

# Cada ficha é uma interpretação didática apoiada nos arquivos indicados.
entries=[
('Movimento com visão superior','Top-down movement','Controle e interação',
 'Permite explorar a cidade em duas dimensões. A animação acompanha a direção em que o corpo realmente se move e mantém a última direção ao parar.',
 'GameInput lê a ação Player/Move. moviment guarda a direção em Update e aplica a velocidade ao Rigidbody2D em FixedUpdate. Os limites consideram o tamanho do colisor. PlayerAnimationController escolhe andar/parado e espelha a arte para a esquerda.',
 ['direcao = lerEntrada("Player/Move")','velocidadeDoCorpo = direcao * velocidadeConfigurada','animacao = escolherEstado(velocidadeDoCorpo)'],
 'A física move o personagem; a animação apenas troca sua aparência, com Root Motion desativado.',
 'Scripts/Player/moviment.cs; Scripts/Player/PlayerAnimationController.cs; Scripts/Core/Input/GameInput.cs'),
('Interação por proximidade','Contextual interaction','Controle e interação',
 'Escolhe o NPC ou objeto próximo que deve responder ao comando de interação. Evita abrir várias ações ao mesmo tempo quando existem alvos vizinhos.',
 'PlayerInteractor registra contatos de colisão e de trigger, filtra quem pode interagir e escolhe o mais próximo. Todos seguem o contrato IInteractable. Ao abrir um painel, um bloqueio impede o movimento até que os donos desse bloqueio o liberem.',
 ['alvo = interagivelValidoMaisProximo()','se apertouInteragir e naoHaModal: alvo.Interact(player)','movimentoBloqueado = quantidadeDeBloqueios > 0'],
 'O mesmo comando pode abrir um prédio, conversar com um NPC ou inspecionar um aventureiro.',
 'Scripts/Core/Input/PlayerInteractor.cs; Scripts/Core/Input/IInteractable.cs'),
('Construção e melhorias','Building upgrades / resource gating','Economia da cidade',
 'Converte ouro e materiais em níveis de edifícios. As melhorias aumentam benefícios e capacidades, conectando a exploração ao desenvolvimento da cidade.',
 'BuildingDefinition guarda preços, requisitos e limites. TryUpgrade valida tudo antes de descontar os custos e aumentar o nível. Se a gravação falhar, restaura os valores. Depois do sucesso, LevelChanged avisa outros sistemas para atualizar seus benefícios.',
 ['se nivel == limite: recusarMelhoria()','se faltamOuroOuMateriais: recusarMelhoria()','consumirCustos(); nivel += 1; salvar(); avisarMudanca()'],
 'Quando não existe um preço específico por nível, o custo usa base × multiplicador elevado ao nível atual.',
 'Scripts/City/BuildingDefinition.cs; Scripts/City/BuildingCalculator.cs; Scripts/City/BuildingService.cs'),
('Renda passiva e impostos','Passive income / timed accrual','Economia da cidade',
 'O Mercado Local acumula dinheiro ao longo do tempo. O jogador precisa recolher os impostos para transformar o valor acumulado em saldo utilizável.',
 'TaxService compara o horário atual com o último cálculo. TaxCalculator converte a produção por minuto em renda pelos segundos elegíveis. Há um limite de 24 horas de acumulação entre coletas. A coleta transfere a parte inteira e preserva a fração restante.',
 ['segundos = limitarTempoElegivelAoTetoDe24Horas()','acumulado += producaoPorMinuto * segundos / 60','ouro += parteInteira(acumulado)'],
 'Ao melhorar o mercado, o tempo anterior é calculado com a produção antiga, evitando aplicar a taxa nova retroativamente.',
 'Scripts/City/TaxAccrualService.cs; Scripts/City/TaxCalculator.cs'),
('Recrutamento de soldados','Unit recruitment / restocking','Recrutamento',
 'Compra guerreiros, arqueiros e magos para a reserva militar. Cada tipo possui preço e estoque; comprar reduz o estoque e aumenta o exército disponível.',
 'RecruitmentService valida quantidade, estoque e dinheiro. A compra e a gravação formam uma operação com restauração em caso de falha. O estoque é reposto a cada seis horas de relógio real; a capacidade da reposição depende do prédio militar correspondente.',
 ['custo = precoUnitario * quantidade','validarEstoqueESaldo(); descontarOuro()','estoque -= quantidade; reserva += quantidade; salvar()'],
 'Preços atuais: guerreiro 3, arqueiro 5 e mago 8. Capacidade de reposição: piso de 10 × (1 + 0,30 × nível).',
 'Scripts/Portao/RecruitmentService.cs'),
('Ofertas de aventureiros','Rotating roster / hero recruitment','Recrutamento',
 'A taverna apresenta ofertas temporárias de personagens individuais. É possível contratar mais de um aventureiro da mesma classe e dar um nome a cada um.',
 'TavernRecruitmentWaveService embaralha o catálogo em ciclos e salva as ofertas. O spawner instancia os prefabs. A compra cria um instanceId próprio, desconta o preço e consome só aquela oferta. Uma nova rodada depende do prazo vencido e de entrar novamente na taverna.',
 ['ofertas = consultarOuSortearRodadaAoEntrar()','personagem = criarRegistroComIdUnico(classe, nome)','pagar(); salvarPersonagem(); consumirUmaOferta()'],
 'A cena Taverna está configurada com 10 s por rodada e 2 ofertas no nível 1, mais 1 por nível adicional.',
 'Scripts/Portao/TavernRecruitmentWaveService.cs; Scripts/Aventureiro/TavernRecruitableSpawner.cs; Scripts/Core/PlayerInventoryService.cs'),
('Deslocamento de tropas','Troop deployment / travel time','Defesa da cidade',
 'Redistribui soldados entre reserva e portões. O tempo de viagem torna importante preparar a defesa antes de a onda chegar.',
 'Dispatch valida a rota e a quantidade, retira as tropas da origem e registra uma transferência com horário de chegada. ResolveArrivals entrega as tropas ao destino quando o relógio estratégico alcança esse horário. O intervalo padrão de viagem é de 15 a 25 segundos estratégicos.',
 ['origem -= quantidade','emTransito.adicionar(destino, quantidade, horarioChegada)','se tempoEstrategico >= chegada: destino += quantidade'],
 'Soldados em trânsito não aparecem simultaneamente na origem e no destino. O registro da viagem permanece no save.',
 'Scripts/Campaign/TroopDeploymentService.cs; Scripts/Campaign/CampaignRules.cs'),
('Defesa por ondas e escalada','Wave-based defense / difficulty scaling','Defesa da cidade',
 'Organiza ataques sucessivos aos portões. A ameaça cresce ao longo da campanha e a composição dos inimigos ganha variedade.',
 'WaveGenerator escolhe portão, prazo e composição usando uma semente de aleatoriedade. CampaignRules calcula um orçamento de ameaça pela onda e aplica um ajuste limitado pelo progresso militar. O gerador gasta esse orçamento em inimigos e impede três ondas seguidas no mesmo portão.',
 ['ameacaBase = arredondar(10 * 1.22^(onda - 1))','ameaca = ameacaBase * fatorDeProgressoLimitado','sortearPortao(); preencherInimigosAteGastarOrcamento()'],
 'A campanha prevê 10 ondas; a primeira tem 180 s de preparação. Brutos entram a partir da terceira e xamãs da quinta.',
 'Scripts/Campaign/WaveGenerator.cs; Scripts/Campaign/WaveDirectorService.cs; Scripts/Campaign/CampaignRules.cs'),
('Resolução estratégica e vantagens','Auto-resolve / unit counters','Defesa da cidade',
 'Resolve o confronto do portão por números: força da guarnição, composição adversária, baixas e dano à estrutura. É diferente da batalha individual de exploração.',
 'ThreatForecastService calcula a defesa efetiva, ponderando vantagens contra os inimigos presentes. BattleCalculator calcula pressão, perdas e dano ao portão. BattleResolutionService aplica e salva o resultado uma vez. Um portão rompido causa derrota; sobreviver à décima onda conclui a campanha com vitória.',
 ['defesa = somarPoderDasTropasComVantagens()','pressao = ameaca / maximo(1, defesa)','aplicarBaixasEDano(); registrarResultado(); salvar()'],
 'Guerreiros favorecem o combate contra saqueadores; arqueiros, contra xamãs; magos, contra brutos. Mesmo uma defesa forte pode sofrer baixas.',
 'Scripts/Campaign/ThreatForecastService.cs; Scripts/Campaign/BattleCalculator.cs; Scripts/Campaign/BattleResolutionService.cs'),
('Antecipação de ameaça','Threat telegraphing / combat forecast','Defesa da cidade',
 'Avisa que um ataque está chegando e apresenta uma estimativa da situação da defesa. Dá ao jogador tempo e informação para decidir onde enviar tropas.',
 'WaveDirectorService compara o prazo com o tempo estratégico e avança os avisos em 90, 30 e 10 segundos restantes. O prognóstico divide defesa efetiva por ameaça. A interface traduz esse número em favorável, arriscado ou desfavorável.',
 ['restante = chegadaDaOnda - tempoEstrategico','razao = defesaEfetiva / ameaca','razao >= 1.15: favoravel; razao < 0.85: desfavoravel'],
 'O intervalo de 0,85 até menos de 1,15 é arriscado. A indicação não significa ausência de baixas nem uma rolagem percentual de vitória.',
 'Scripts/Campaign/WaveDirectorService.cs; Scripts/Campaign/ThreatForecastService.cs; Scripts/Campaign/ThreatPresentationService.cs'),
('Expedições e composição de grupo','Party management / mission state machine','Exploração',
 'Envia de um a três aventureiros para capturar ou defender uma região. Um personagem ocupado não pode participar de outra missão ou guarda ao mesmo tempo.',
 'TrySendParty verifica contratação, destino, tamanho e disponibilidade. Cria uma cópia dos atributos para a missão, chamada snapshot, e registra a chegada. Os estados controlam o fluxo: viajando, pronto para batalha, em batalha, retornando e retornado; derrota e retirada são saídas alternativas.',
 ['validarGrupoEDestino()','missao.party = copiarAtributosDosSelecionados()','missao.status = Travelling; chegada = agora + 5'],
 'Há uma expedição ativa por vez. A cópia dos atributos protege uma viagem já iniciada de alterações posteriores na definição do personagem.',
 'Scripts/Exploration/ExplorationService.cs; Scripts/Exploration/ExplorationTypes.cs'),
('Desbloqueio de regiões','Progression gating / prerequisite graph','Exploração',
 'Libera destinos conforme as conquistas anteriores. Algumas regiões exigem uma vitória prévia; outras exigem mais de uma.',
 'ExplorationProgression mantém um dicionário: cada destino aponta para seus pré-requisitos. IsUnlocked só retorna verdadeiro quando todos aparecem na lista de regiões concluídas. A mesma regra é consultada pela seleção no mapa e pela validação do envio.',
 ['para cada requisito do destino:','    se requisito naoEstaConcluido: retornar bloqueado','retornar liberado'],
 'Reino Real exige Floresta Esmeralda e Montanhas Gélidas. Colinas do Vento, Floresta das Sombras e Caverna do Gelo começam acessíveis.',
 'Scripts/Exploration/ExplorationProgression.cs; Scripts/Exploration/ExplorationService.cs'),
('Combate automático por iniciativa','Automatic turn-based combat / initiative meter','Batalha de exploração',
 'Os combatentes agem automaticamente. A velocidade influencia a frequência das ações, em vez de apenas ordenar uma rodada fixa.',
 'ExplorationTurnBattle mantém um medidor por unidade. Quando ninguém está pronto, soma a velocidade dos vivos até alguém atingir 100. Quem age gasta 100 pontos. Empates consideram medidor, velocidade, equipe e nome. O controlador apresenta as ações com intervalo de 0,65 s.',
 ['enquanto ninguemTem100: iniciativa += maximo(1, velocidade)','atacante = escolherProntoPorPrioridade()','iniciativa[atacante] -= 100; executarAtaque()'],
 'Ao longo da luta, velocidade 20 permite aproximadamente o dobro de ações de velocidade 10. O jogador inicia a batalha ou recua; não escolhe cada golpe.',
 'Scripts/Exploration/ExplorationTurnBattle.cs; Scripts/Exploration/ExplorationCombatSceneController.cs'),
('Formação e proteção da linha de frente','Frontline formation / target priority','Batalha de exploração',
 'Coloca os mais resistentes na frente e concentra os ataques no primeiro oponente vivo daquela formação.',
 'No início, as equipes são ordenadas pela soma de armadura e resistência mágica, com vida máxima como desempate. Aliados mais resistentes ficam à direita; inimigos, à esquerda. Os alvos são o inimigo vivo mais à esquerda ou o aliado vivo mais à direita. Receber dano não reordena a equipe.',
 ['resistencia = armadura + resistenciaMagica','ordenarFormacao(resistencia, vidaMaxima)','alvo = primeiroVivoDaLinhaDeFrenteAdversaria()'],
 'Um tanque protege quem está atrás porque a regra de alvo o escolhe primeiro. O espaço visual também inclui sprite, barra inteira e nome, mais a margem do desenvolvedor.',
 'Scripts/Exploration/ExplorationTurnBattle.cs; Scripts/Exploration/ExplorationCombatSceneController.cs; Scripts/Exploration/ExplorationCombatantView.cs'),
('Atributos, vida e redução de dano','Stat scaling / damage mitigation','Batalha de exploração',
 'Diferencia os personagens por vida, velocidade, dano e defesas. Ataques físicos e mágicos consultam proteções diferentes.',
 'AdventurerStatsCalculator soma os bônus de vida dos equipamentos e calcula dano por atributos multiplicados pelos escaladores. O motor sorteia um valor entre dano mínimo e máximo. ExplorationCombatCalculator subtrai armadura ou resistência mágica e garante pelo menos 1 de dano.',
 ['defesa = ataqueMagico ? resistenciaMagica : armadura','danoFinal = maximo(1, danoSorteado - defesa)','vidaAtual = maximo(0, vidaAtual - danoFinal)'],
 'Exemplo: 12 de dano físico contra 5 de armadura causa 7. A soma das duas defesas organiza a formação, mas não é subtraída de cada ataque.',
 'Scripts/Aventureiro/AdventurerStats.cs; Scripts/Exploration/ExplorationTypes.cs; Scripts/Exploration/ExplorationTurnBattle.cs'),
('Encontros variáveis e tabelas de saque','Constrained random encounters / loot tables','Batalha de exploração',
 'Varia o grupo inimigo dentro dos limites do local e sorteia os recursos recebidos. O mesmo sistema de combate serve a vários ambientes.',
 'ExplorationEncounterDefinition lista combinações de até três inimigos e filtra pela quantidade e pelo poder permitidos. Sorteia uma combinação válida com uma semente. Cada recompensa possui recurso, chance e intervalo de quantidade. A vitória registra o saque; a entrega ao inventário ocorre no retorno.',
 ['gruposValidos = filtrarPorQuantidadeEPoder(combinacoes)','inimigos = sortear(gruposValidos, sementeDaMissao)','se sorteioPassaNaChance: quantidade = sortear(minimo, maximo)'],
 'Uma configuração impossível usa a combinação mais próxima e emite aviso. Defesa pode reduzir a faixa de poder conforme o asset do encontro.',
 'Scripts/Exploration/ExplorationEncounterDefinition.cs; Scripts/Exploration/ExplorationCombatSceneController.cs; Scripts/Exploration/ExplorationService.cs'),
('Alocação de trabalhadores e coleta','Worker allocation / passive resource gathering','Gestão de territórios',
 'Distribui exploradores por áreas capturadas para gerar recursos em ciclos. É preciso escolher onde empregar a capacidade disponível.',
 'O total corresponde ao nível da Construção de Exploradores menos as perdas. O disponível desconta os trabalhadores já alocados. A cada ciclo, o serviço faz um sorteio da tabela por explorador e acumula os resultados no local. Áreas com ataque pendente não produzem.',
 ['total = maximo(0, nivelDoAlojamento - exploradoresPerdidos)','disponiveis = maximo(0, total - alocados)','porCiclo: sortearRecompensasParaCadaExplorador()'],
 'O intervalo padrão no serviço é de 300 s e pode ser configurado pelo indicador da taverna. Há limite de 100 ciclos processados por chamada de atualização.',
 'Scripts/Exploration/ExplorationService.cs; Scripts/Exploration/TavernActionAttentionIndicator.cs'),
('Conquista, guarnição e contra-ataque','Territory control / garrison defense','Gestão de territórios',
 'Uma região conquistada passa a produzir, mas precisa de proteção. Os contra-ataques criam risco para a expansão e para os exploradores enviados.',
 'A vitória marca a área como capturada e agenda um ataque para 300 s depois. O jogador pode deixar de um a três aventureiros de guarda. Ao vencer o prazo, uma guarda permite criar uma missão de defesa; sem guarda, a área é perdida e os exploradores alocados entram na contagem de perdas.',
 ['aoVencer: capturada = verdadeiro; ataque = agora + 300','aoChegarAtaque: se temGuarda, prepararDefesa()','senao: perderArea(); registrarExploradoresPerdidos()'],
 'A perda sem guarda também limpa o saque pendente do local. Uma defesa pendente interrompe a coleta até a situação ser resolvida.',
 'Scripts/Exploration/ExplorationService.cs: CompleteVictory, RefreshAreaThreats e LoseUndefendedArea'),
('Inventário e transporte de recursos','Resource inventory / collection and delivery','Recursos e informação',
 'Separa o que está disponível na cidade do que foi coletado e ainda está na região. O jogador precisa transportar a produção dos exploradores para poder utilizá-la.',
 'Os materiais são registros com ID e quantidade. pendingWorkerLoot guarda a produção local; TryTransportWorkerLoot soma esses valores ao inventário e limpa a lista pendente. O saque de missão segue outra regra: é entregue ao retornar, com rewardsClaimed impedindo uma segunda entrega.',
 ['para cada recurso pendente: inventario[id] += quantidade','limparRecursosPendentes(); salvar()','noRetornoDaMissao: entregarSomenteSeAindaNaoRecebido()'],
 'Transportar funciona como uma transferência de valores no código atual; esse botão não inicia uma caravana animada nem um novo prazo de viagem.',
 'Scripts/Core/PlayerInventoryService.cs; Scripts/Exploration/ExplorationService.cs'),
('Relatório de área e retorno visual','HUD feedback / data-driven UI','Recursos e informação',
 'Mostra o estado real do local: recursos, trabalhadores, guarda, vida e próximo ataque. Ajuda o jogador a entender o resultado das suas decisões.',
 'ExplorationAreaReportView lê o serviço, instancia linhas com o prefab de item e usa ícone e nome do recurso. Exibe D como produção pendente e T como máximo por ciclo multiplicado pelos exploradores. Liga apenas os slots ocupados e atualiza os textos em intervalos de 0,2 s.',
 ['D = quantidadeColetadaAguardandoTransporte','T = maximoDoRecursoPorCiclo * exploradoresNoLocal','tempoExibido = minutos(restante) + ":" + segundos(restante)'],
 'No contador de pessoas, “2 / 3” significa 2 no local e 3 ainda disponíveis globalmente. T é um potencial máximo, não uma promessa de saque nem limite de estoque.',
 'Scripts/Exploration/ExplorationAreaReportView.cs; Scripts/Exploration/ExplorationRewardItemView.cs'),
('Câmera de acompanhamento e limites','Camera follow / damping / camera confiner','Sistemas de apresentação',
 'Mantém o personagem enquadrado com suavização e restringe a câmera ao mapa. É um sistema de apresentação que afeta a sensação de controle.',
 'A cena Centro usa CinemachineFollow para seguir o alvo e CinemachineConfiner2D para respeitar o limite. O Rigidbody2D do jogador tem interpolação. No ajuste atual, o damping horizontal e vertical é 0,15 e o tamanho ortográfico é 5. O chão usa Bilinear para suavizar a amostragem durante o deslocamento.',
 ['posicaoDesejada = posicaoDoAlvo + deslocamentoDaCamera','camera = aproximarSuavemente(posicaoDesejada)','camera = restringirAoLimiteDoMapa(camera)'],
 'Esses valores descrevem o Centro. Não significam que todas as cenas usem a mesma câmera, nem que exista Pixel Perfect Camera configurada.',
 'Scenes/Centro.unity; Prefabs/player.prefab; Images/Map/CentroStaticV3.png.meta'),
('Profundidade e transparência por oclusão','Y-sorting / occlusion fading','Sistemas de apresentação',
 'Faz o personagem aparecer à frente ou atrás dos objetos de forma coerente. Certos objetos ficam transparentes quando encobrem o jogador.',
 'WorldPrototypeDepthSort calcula a ordem de desenho pela altura dos pés, usando um ponto estável do colisor. Quanto menor o Y, maior a prioridade de desenho. WorldPropOcclusion verifica posição e sobreposição e reduz o alpha do sprite para 0,35 quando o jogador está atrás.',
 ['ordem = 2000 - arredondar(yDosPes * 100) + ajuste','se jogadorEstaAtras e imagensSeSobrepoem: alpha = 0.35','senao: alpha = 1'],
 'A referência nos pés evita que uma animação mais alta faça o personagem mudar de camada. O efeito depende dos componentes presentes em cada objeto.',
 'Scripts/Prototypes/WorldPrototypeDepthSort.cs; Scripts/Prototypes/WorldPropOcclusion.cs'),
('Viagem entre cenas e pontos de entrada','Scene transitions / spawn points','Sistemas de suporte',
 'Conecta Centro, portões, taverna e combate sem perder o contexto do destino. O personagem aparece no ponto de entrada adequado.',
 'SceneTransitionController bloqueia uma segunda transição, escurece a tela, pausa o relógio estratégico e carrega a cena de modo assíncrono. PlayerSpawnController posiciona o jogador em uma âncora. Depois, a tela clareia e a pausa é liberada. A cena de portão reutiliza o contexto do portão escolhido.',
 ['bloquearNovaViagem(); pausarCampanha(); escurecer()','carregarCenaAsync(destino); posicionarNaAncora()','clarear(); liberarPausa(); liberarNovaViagem()'],
 'O controlador usa DontDestroyOnLoad para continuar existindo durante a troca. A duração padrão de cada etapa do fade é de 0,2 s.',
 'Scripts/Navigation/SceneTransitionController.cs; Scripts/Navigation/PlayerSpawnController.cs; Scripts/Navigation/NavigationService.cs'),
('Salvamento e estado persistente','Save/load / persistence','Sistemas de suporte',
 'Mantém dinheiro, construções, exército, aventuras, regiões e outros progressos entre sessões. Compartilha esses dados entre as cenas.',
 'GameSession reúne os serviços sobre um GameSaveData. Commit pede a gravação ao repositório. GameSaveRepository transforma os dados em JSON e grava em PlayerPrefs. Ao carregar, normaliza valores, migra formatos antigos e preserva uma cópia de dados inválidos antes de recuperar um estado padrão.',
 ['dados = repositorio.Load()','servicos = criarServicosUsandoOsMesmosDados(dados)','aoConfirmarMudanca: repositorio.Save(dados)'],
 'É um save local. A chave atual é tcc.game.save.v2; isso não representa sincronização em nuvem nem retomada exata de cada estado interno da luta.',
 'Scripts/Core/GameSession.cs; Scripts/Core/Persistence/GameSaveData.cs; Scripts/Core/Persistence/GameSaveRepository.cs'),
]

def entry(number, item, y):
    title,term,category,what,how,pseudo,example,sources=item
    start=y
    line(f'{number:02d}',44,y-16,20,'Bold',TEAL)
    y=para(title,83,y,W-127,15.5,19,font='Bold')
    y=para(term,83,y-4,W-127,9.3,12,color=GOLD)-13
    y=para('<b>O que faz.</b> '+escape(what),44,y,W-88)-8
    y=para('<b>Como foi implementado.</b> '+escape(how),44,y,W-88)-9
    y=code(pseudo,y)
    y=para('<b>Para entender:</b> '+escape(example),44,y,W-88,9.7,13)-9
    y=para('<b>No projeto:</b> '+escape(sources),44,y,W-88,8,10,color=MUTED)-10
    assert y>52, f'Overflow page {PAGE}: entry {number}, bottom {y}'
    return y

# 1. Capa com visão do ciclo do jogo.
PAGE+=1
c.setFillColor(INK);c.rect(0,0,W,H,fill=1,stroke=0)
c.setFillColor(TEAL);c.rect(0,H-14,W,14,fill=1,stroke=0)
line('TCC-GAME  /  DOCUMENTAÇÃO DE GAMEPLAY',48,785,9,'Bold',HexColor('#8FD7D1'))
para('As mecânicas<br/>do nosso jogo',48,718,W-96,40,47,white,'Bold')
para('O nome, o que cada uma faz e como o código funciona.',48,599,W-105,18,25,HexColor('#D9E9E8'))
para('24 mecânicas e sistemas de suporte explicados com exemplos do projeto, termos usados em jogos e pseudocódigo em linguagem simples.',48,515,W-108,12,18,HexColor('#D9E9E8'))
for i,(label,desc) in enumerate([('DESENVOLVER','Construir e obter recursos'),('PREPARAR','Recrutar e distribuir equipes'),('ENFRENTAR','Defender portões e explorar'),('EXPANDIR','Conquistar e sustentar regiões')]):
    y=400-i*66
    c.setFillColor(HexColor('#254752'));c.roundRect(48,y-48,W-96,54,6,fill=1,stroke=0)
    line(f'{i+1:02d}',62,y-16,16,'Bold',HexColor('#8FD7D1'))
    line(label,102,y-10,9,'Bold',HexColor('#DDB481'))
    line(desc,102,y-29,12,color=white)
line('Análise do projeto local em 10 de novembro de 2026',48,79,9,color=HexColor('#B9D0D1'))
line('Preparado por Lain para Gustavo',48,58,9,color=HexColor('#B9D0D1'))
c.bookmarkPage('p1');c.addOutlineEntry('As mecânicas do nosso jogo','p1',0,False)
end()

# 2. Orientação e índice.
y=page('Como ler este guia','Visão geral')
y=block('O que foi analisado','A análise parte dos scripts C# atuais e das configurações salvas nas cenas. Os nomes em inglês são termos usuais ou descrições equivalentes; não pressupõem que o projeto copie exatamente a implementação de outro jogo.',y)
y=block('Dois combates, duas regras','<b>Portões:</b> soldados em quantidade, defesa efetiva e resultado automático por cálculo.<br/><b>Exploração:</b> aventureiros individuais, formação, vida e ações automáticas por iniciativa. Os dois sistemas não compartilham a mesma fórmula de batalha.',y)
rows=[('03','Controle e interação','01-02'),('04','Economia da cidade','03-04'),('05','Recrutamento','05-06'),('06','Defesa: tropas e ondas','07-08'),('07','Defesa: resultados e avisos','09-10'),('08','Exploração e desbloqueios','11-12'),('09','Iniciativa e formação','13-14'),('10','Atributos, encontros e saque','15-16'),('11','Produção e territórios','17-18'),('12','Inventário e relatório','19-20'),('13','Câmera e profundidade','21-22'),('14','Cenas e salvamento','23-24'),('15','Relógios e organização do código',''),('16','Vocabulário e limites atuais','')]
y=para('ROTEIRO DE LEITURA',44,y,W-88,8.5,12,TEAL,'Bold')-8
for pg,title,ids in rows:
    line(pg,44,y-11,10,'Bold',TEAL);line(title,78,y-11,10)
    c.setFont('Body',9);c.setFillColor(MUTED);c.drawRightString(W-44,y-11,ids)
    c.linkRect('',f'p{int(pg)}',(44,y-15,W-44,y+2),relative=0,thickness=0)
    y-=23
y-=13
y=para('<b>Sobre os exemplos.</b> Os quadros são pseudocódigo: resumem a lógica e não devem ser colados diretamente no C#. Os valores indicados são da revisão analisada e podem mudar no Inspector ou nos assets.',44,y,W-88,9.7,14)-10
y=para('<b>Como localizar os arquivos.</b> As referências das fichas são relativas à pasta <b>Assets</b> do projeto <b>TCC-Game</b>. Pastas chamadas Prototypes ainda contêm componentes usados no jogo.',44,y,W-88,9.7,14)
assert y>52,y
end()

titles=['Controle e interação','Economia da cidade','Recrutamento','Defesa: tropas e ondas','Defesa: resultados e avisos','Exploração e desbloqueios','Iniciativa e formação','Atributos, encontros e saque','Produção e territórios','Inventário e relatório','Câmera e profundidade','Cenas e salvamento']
for pair in range(12):
    y=page(titles[pair], 'Fichas '+str(pair*2+1).zfill(2)+' e '+str(pair*2+2).zfill(2))
    y=entry(pair*2+1,entries[pair*2],y)
    c.setStrokeColor(RULE);c.line(44,y,W-44,y)
    y=entry(pair*2+2,entries[pair*2+1],y-18)
    end()

# 15. Ligação didática entre as funcionalidades e o código.
y=page('Tempo e organização do código','Entendendo as ligações')
y=block('1. Relógio estratégico','CampaignRuntimeDriver acrescenta <b>Time.unscaledDeltaTime</b> ao tempo da campanha. Ondas e viagens de tropas usam esse contador. Ele não avança quando a campanha está pausada nem depois do seu encerramento. As transições de cena solicitam e depois liberam essa pausa.',y)
y=code(['se campanhaPodeAvancar:','    tempoEstrategico += tempoDoFrame','    resolverChegadas(); atualizarOndas(); salvarSeNecessario()'],y)-8
y=block('2. Relógio real','<b>IClock / SystemClock</b> fornece o horário em segundos Unix. Impostos, reposição de soldados, validade das ofertas e prazos de exploração consultam esse horário. Ao serem atualizados, podem reconhecer tempo decorrido fora daquela tela ou com o jogo fechado. Isso não significa que toda a simulação rode continuamente em segundo plano.',y)
y=para('<b>Detalhe do projeto:</b> na exploração, a atualização verifica ameaças antes da produção. Um ataque pendente pode bloquear a coleta, e uma área sem guarda pode ser perdida antes de calcular ciclos atrasados. Não se deve prometer produção offline irrestrita.',44,y,W-88,10.3,14)-20
y=block('3. A tela pede; o serviço decide','Um botão chama um método, como TryUpgrade. O serviço valida as regras, altera os dados e chama Commit. A interface lê o resultado ou recebe um evento para redesenhar os textos. Isso evita esconder a regra de compra dentro do desenho do botão.',y)
y=code(['Botao -> Controller -> Service -> GameSaveData','Service -> Commit -> Repository -> JSON em PlayerPrefs','Resultado ou evento -> View -> texto, imagem e botoes'],y)-8
y=block('4. Por que usar definições separadas','Char_base, BuildingDefinition e ExplorationEncounterDefinition são <b>ScriptableObjects</b>: fichas de configuração editáveis no Unity. O estado da partida fica separado. Dois guerreiros podem usar a mesma definição, mas ter IDs de contratação e nomes diferentes.',y)
y=para('<b>Fontes:</b> Scripts/Core/GameSession.cs; Scripts/Core/Time/IClock.cs; Scripts/Campaign/CampaignRuntimeDriver.cs; Scripts/Campaign/CampaignService.cs; Scripts/Exploration/ExplorationService.cs.',44,y,W-88,8,11,color=MUTED)
assert y>52,y
end()

# 16. Glossário e distinções entre implementação e campos ainda não usados.
y=page('Vocabulário e limites atuais','Para consultar enquanto lê o C#')
terms=[('Classe e método','Uma classe reúne responsabilidades; um método executa uma ação. Exemplo: ExplorationService é a classe e TrySendParty é um método.'),('Variável, bool e enum','Variável guarda um valor. bool significa sim/não. enum lista opções nomeadas, como Travelling, InBattle e Returned.'),('MonoBehaviour e ciclo de atualização','Componente ligado a um GameObject. Update roda por frame; FixedUpdate acompanha a física; LateUpdate executa depois de Update.'),('Prefab e Instantiate','Prefab é um modelo reutilizável. Instantiate cria uma cópia, como uma oferta de aventureiro ou uma linha no relatório.'),('Evento e coroutine','Evento avisa que algo mudou. Coroutine distribui uma sequência ao longo de frames; é usada na apresentação do combate e nas transições.'),('Snapshot, seed e bounds','Snapshot é uma cópia dos dados naquele momento. Seed é a semente do sorteio. Bounds são limites espaciais usados para medir e posicionar objetos.')]
for term,desc in terms:
    y=para('<b>'+escape(term)+'.</b> '+escape(desc),44,y,W-88,10.1,14)-11
y-=7
y=para('O QUE NÃO DEVE SER CONFUNDIDO COM MECÂNICA PRONTA',44,y,W-88,9,13,TEAL,'Bold')-9
notes=[
 '<b>Traços e mana:</b> Tracos está vazio. mana e ganho_mana existem na definição, mas não encontrei consumo de mana, habilidades ou efeitos de traços no motor de combate analisado.',
 '<b>Equipamentos:</b> o cálculo atual soma HP dos slots e usa os atributos base para o dano. Os campos de força, destreza e inteligência do Item não são aplicados por AdventurerStatsCalculator; não equivale a um sistema completo de equipamento em runtime.',
 '<b>Morte e recuperação:</b> chegar a zero de vida tira a unidade daquela luta. Não identifiquei remoção permanente do aventureiro contratado. A criação de snapshots e das equipes de defesa inicializa a vida máxima.',
 '<b>Outras mecânicas:</b> não identifiquei salto/coyote time, progressão por XP ou árvore de habilidades nos scripts examinados. Esses nomes não foram incluídos como funcionalidades existentes.'
]
for note in notes:y=para(note,44,y,W-88,9.7,13.5)-10
y=para('<b>Escopo:</b> este documento é uma análise do código e das configurações locais, não uma certificação de todos os fluxos em runtime. Não foram alteradas regras do jogo para produzir o guia.',44,y-4,W-88,9,12.5,color=MUTED)
assert y>52,y
end()
c.save()
print(OUT)
print(f'{PAGE} pages')
