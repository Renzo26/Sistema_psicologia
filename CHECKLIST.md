# CHECKLIST — Sistema de Gestão para Clínicas de Psicologia

> Marque `[x]` ao concluir cada tarefa. Atualize este arquivo a cada sessão de desenvolvimento.
> Stack: .NET Core 8 | Angular 18+ | PostgreSQL 16 | N8N

---

## FASE 0 — Infraestrutura & Setup Inicial

### 0.1 — Estrutura do Projeto
- [x] Criar solution .NET com estrutura de pastas (Api, Domain, Application, Infrastructure, Tests)
- [x] Configurar projeto Angular 18+ com lazy-loaded modules
- [x] Criar scripts SQL para todas as tabelas (pasta `/sql`) — Supabase + N8N já hospedados na VPS
- [x] Configurar `.gitignore`, `.editorconfig` e `README.md`

### 0.2 — Configurações Base .NET
- [x] Instalar pacotes NuGet: MediatR, FluentValidation, EF Core, Hangfire, JWT Bearer
- [x] Instalar pacotes NuGet: xUnit, FluentAssertions, Testcontainers
- [x] Configurar `appsettings.json` e `appsettings.Development.json`
- [x] Configurar Serilog para logging estruturado
- [x] Configurar ProblemDetails (RFC 7807) para respostas de erro
- [x] Configurar rate limiting (100 req/min público, 300 req/min autenticado)
- [x] Configurar CORS para o frontend Angular

### 0.3 — Configurações Base Angular
- [x] Configurar `ApiService` centralizado (HttpClient)
- [x] Configurar interceptor de JWT (Bearer token)
- [x] Configurar interceptor de refresh token (cookie HttpOnly)
- [x] Configurar NgRx Signal Store para estado global
- [x] Configurar estratégia OnPush em todos os componentes
- [x] Configurar guards de rota (autenticação)
- [x] Configurar tratamento global de erros HTTP

### 0.4 — Banco de Dados & Multi-tenancy
- [x] Configurar EF Core com PostgreSQL (snake_case naming)
- [x] Implementar estratégia multi-tenant: schema por tenant
- [x] Criar `TenantMiddleware` para resolver tenant via JWT/subdomínio
- [x] Implementar `TenantDbContext` com filtros globais por tenant
- [x] Criar mecanismo de criação automática de schema por novo tenant
- [x] Configurar soft delete global (`excluido_em`)
- [x] Configurar TIMESTAMPTZ para todos os campos de data

---

## FASE 1 — Autenticação & Multi-tenancy

### 1.1 — Autenticação (Auth)
- [x] Criar entidade `Usuario` com roles (Admin, Gerente, Secretaria, Psicologo)
- [x] Implementar registro de usuário com hash de senha (BCrypt)
- [x] Implementar login com geração de JWT (access 15min + refresh 7d)
- [x] Implementar refresh token via cookie HttpOnly
- [x] Implementar logout (invalidar refresh token)
- [x] Implementar troca de senha
- [x] Implementar recuperação de senha (email)
- [x] Criar tela de Login no Angular
- [x] Criar tela de Recuperação de Senha no Angular
- [x] Testes unitários de autenticação (90% cobertura domain)

### 1.2 — Gestão de Tenants (Clínicas)
- [x] Criar entidade `Clinica` (tenant raiz)
- [x] Implementar criação de novo tenant (schema PostgreSQL)
- [x] Implementar isolamento de dados por tenant em todas as queries
- [x] Criar endpoint de onboarding inicial (setup da clínica)
- [x] Criar checklist de onboarding no Angular (<30 min meta)
- [x] Implementar auditoria: log de todas as ações financeiras
- [x] Testes de isolamento multi-tenant

---

## MÓDULO 1 — Cadastros

### 1.1 — Clínica
- [x] Criar entidade `Clinica` (nome, CNPJ, endereço, telefone, email)
- [x] Command: `CriarClinicaCommand` + Handler + Validator
- [x] Command: `AtualizarClinicaCommand` + Handler + Validator
- [x] Query: `ObterClinicaQuery` + Handler
- [x] Endpoint REST: `POST /clinicas`, `PUT /clinicas/minha`, `GET /clinicas/minha`
- [x] Tela de cadastro/edição de clínica no Angular
- [x] Testes unitários e de integração (32 testes aprovados)

### 1.2 — Psicólogos
- [x] Criar entidade `Psicologo` (nome, CRP, tipo PJ/CLT, percentual ou valor fixo repasse)
- [x] Command: `CriarPsicologoCommand` + Handler + Validator
- [x] Command: `AtualizarPsicologoCommand` + Handler + Validator
- [x] Command: `InativarPsicologoCommand` + Handler
- [x] Query: `ListarPsicologosQuery` + Handler (com filtros)
- [x] Query: `ObterPsicologoQuery` + Handler
- [x] Endpoints REST: CRUD completo `/psicologos`
- [x] Tela de listagem de psicólogos no Angular
- [x] Tela de cadastro/edição de psicólogo no Angular
- [x] Testes unitários e de integração (100 testes aprovados)

### 1.3 — Pacientes
- [x] Criar entidade `Paciente` (nome, CPF, email, telefone, data nascimento)
- [x] Implementar criptografia de dados sensíveis (LGPD) — AES-256 via IEncryptionService
- [x] Command: `CriarPacienteCommand` + Handler + Validator
- [x] Command: `AtualizarPacienteCommand` + Handler + Validator
- [x] Command: `InativarPacienteCommand` + Handler
- [x] Query: `ListarPacientesQuery` + Handler (com busca/filtros)
- [x] Query: `ObterPacienteQuery` + Handler
- [x] Endpoints REST: CRUD completo `/pacientes`
- [x] Tela de listagem de pacientes no Angular
- [x] Tela de cadastro/edição de paciente no Angular
- [x] Testes unitários e de integração (100 testes aprovados)

### 1.4 — Contratos
- [x] Criar entidade `Contrato` (paciente, psicólogo, valor sessão, dia semana, frequência, forma pgto)
- [x] Criar enum `StatusContrato` (Ativo, Pausado, Encerrado)
- [x] Criar enum `FormaPagamento` (PIX, Cartão, Dinheiro, Convênio)
- [x] Criar enum `FrequenciaContrato` (Semanal, Quinzenal)
- [x] Command: `CriarContratoCommand` + Handler + Validator
- [x] Command: `AtualizarContratoCommand` + Handler + Validator
- [x] Command: `EncerrarContratoCommand` + Handler
- [x] Query: `ListarContratosQuery` + Handler
- [x] Query: `ObterContratoQuery` + Handler
- [x] Endpoints REST: CRUD completo `/contratos`
- [x] Tela de listagem de contratos no Angular
- [x] Tela de cadastro/edição de contrato no Angular
- [x] Testes unitários e de integração (119 testes aprovados)

### 1.5 — Planos de Conta (Financeiro)
- [x] Criar entidade `PlanoConta` (nome, tipo Receita/Despesa, categoria)
- [x] CRUD completo + endpoints REST (`GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`)
- [x] Tela de gestão de planos de conta no Angular (listagem + modal criar/editar)
- [x] Testes unitários (10 testes aprovados)

---

## MÓDULO 2 — Sessões

### 2.1 — Agendamento de Sessões
- [x] Criar entidade `Sessao` (contrato, data, hora, psicólogo, paciente, status)
- [x] Criar enum `StatusSessao` (Agendada, Realizada, Falta, FaltaJustificada, Cancelada)
- [x] Regra: sessão só pode mudar status dentro de 30 dias (exceto Admin)
- [x] Command: `AgendarSessaoCommand` + Handler + Validator
- [x] Command: `AtualizarSessaoCommand` + Handler + Validator
- [x] Command: `CancelarSessaoCommand` + Handler

### 2.2 — Recorrência Semanal/Quinzenal
- [x] Implementar `GerarSessoesRecorrentesCommand` (a partir do contrato)
- [x] Serviço de domínio: calcular próximas N sessões com base na frequência
- [x] Hangfire job: gerar sessões do mês seguinte automaticamente (`GerarSessoesMesSeguinteJob`)
- [x] Regra: ao gerar sessão, criar lançamento financeiro com status `Previsto`

### 2.3 — Controle de Frequência
- [x] Command: `MarcarPresencaCommand` + Handler
- [x] Command: `RegistrarFaltaCommand` (justificada/não justificada) + Handler
- [x] Regra de negócio: falta justificada não gera cobrança (configurável por contrato)
- [x] Query: `ListarSessoesQuery` + Handler (filtros: data, psicólogo, paciente, status)
- [x] Query: `ObterSessaoQuery` + Handler

### 2.4 — Endpoints & Frontend
- [x] Endpoints REST: `/sessoes` CRUD completo
- [x] Endpoint: `POST /sessoes/gerar-recorrentes`
- [x] Tela de agenda/calendário de sessões no Angular
- [x] Tela de listagem de sessões com filtros no Angular
- [x] Marcação de presença/falta inline na listagem
- [x] Testes unitários e de integração (regras de recorrência e status)

---

## MÓDULO 3 — Financeiro

### 3.1 — Lançamentos Financeiros
- [x] Criar entidade `LancamentoFinanceiro` (descrição, valor, tipo, status, vencimento, competência)
- [x] Criar enum `TipoLancamento` (Receita, Despesa)
- [x] Criar enum `StatusLancamento` (Previsto, Confirmado, Cancelado)
- [x] Criar Domain Event: `SessaoRealizadaEvent` → confirmar lançamento
- [x] Criar Domain Event: `SessaoCanceladaEvent` → cancelar lançamento
- [x] Command: `CriarLancamentoCommand` + Handler + Validator
- [x] Command: `AtualizarLancamentoCommand` + Handler + Validator
- [x] Command: `ConfirmarPagamentoCommand` + Handler
- [x] Command: `CancelarLancamentoCommand` + Handler
- [x] Query: `ListarLancamentosQuery` + Handler (filtros: período, tipo, status)
- [x] Query: `ObterFluxoCaixaQuery` (diário/semanal/mensal, previsto vs realizado)

### 3.2 — Repasses para Psicólogos PJ
- [x] Serviço de domínio: `CalcularRepasseService` (percentual ou valor fixo)
- [x] Command: `GerarRepasseMensalCommand` + Handler
- [x] Criar entidade `Repasse` (psicólogo, mês, valor calculado, status)
- [x] Endpoint: `POST /repasses/calcular`
- [x] Query: `ListarRepassesQuery` + Handler

### 3.3 — Fechamento Mensal
- [x] Command: `RealizarFechamentoMensalCommand` + Handler
- [x] Regra: período fechado não permite edições de lançamentos
- [x] Criar entidade `FechamentoMensal` (mês, totais, status)
- [x] Gerar relatório consolidado por psicólogo no fechamento
- [x] Endpoint: `POST /fechamentos`
- [x] Query: `ObterFechamentoQuery` + Handler
- [x] Query: `ListarFechamentosQuery` + Handler

### 3.4 — Endpoints & Frontend
- [x] Endpoints REST: `/lancamentos` CRUD completo
- [x] Endpoints REST: `/repasses` (CRUD + pagar)
- [x] Endpoints REST: `/fechamentos`
- [x] Tela de lançamentos financeiros com filtros no Angular
- [x] Tela de fluxo de caixa (visão diária/mensal, previsto vs realizado) no Angular
- [x] Tela de repasses por psicólogo no Angular
- [x] Tela de fechamento mensal no Angular
- [x] Testes unitários e de integração (regras de repasse: 6 testes aprovados)

---

## MÓDULO 4 — Automações N8N

### 4.1 — Infraestrutura de Webhooks
- [ ] Criar serviço `WebhookDispatcherService`
- [ ] Criar entidade `LogAutomacao` (evento, payload, status, resposta, timestamp)
- [ ] Implementar retry automático em caso de falha (Hangfire)
- [ ] Endpoint interno: `POST /webhooks/n8n` (receber callbacks do N8N)
- [ ] Criar variáveis de ambiente para URLs dos webhooks N8N

### 4.2 — Cobrança Automática via WhatsApp
- [ ] Webhook: `cobranca.vencida` (disparado por Hangfire job diário)
- [ ] Job: verificar lançamentos vencidos e não pagos, disparar webhook
- [ ] Payload: dados do paciente, valor, vencimento, link de pagamento
- [ ] Log de cada mensagem enviada com status

### 4.3 — Alertas para o Gestor
- [ ] Job diário (manhã): compilar resumo do dia (caixa, sessões, pendências)
- [ ] Webhook: `alerta.diario.gestor`
- [ ] Configurar horário de envio (configurável por clínica)

### 4.4 — Notificação de Repasse
- [ ] Webhook: `repasse.calculado` (disparado ao gerar repasse mensal)
- [ ] Payload: psicólogo, valor, mês de referência
- [ ] Tela de configuração de automações no Angular (ativar/desativar, URLs)
- [ ] Tela de log de automações no Angular
- [ ] Testes de integração dos webhooks

---

## MÓDULO 5 — Dashboard & Métricas

### 5.1 — KPIs e Cálculos
- [x] Query: `ObterKpisDashboardQuery` (período configurável)
- [x] KPI: taxa de absenteísmo (faltas / sessões agendadas)
- [x] KPI: taxa de inadimplência (previsto não recebido / previsto total)
- [x] KPI: ticket médio por sessão
- [x] KPI: receita realizada vs projetada (mensal)
- [x] KPI: sessões realizadas vs agendadas
- [x] KPI: ranking de psicólogos por volume de sessões/receita

### 5.2 — Relatórios
- [x] Query: `RelatorioFluxoCaixaQuery` (exportável)
- [x] Query: `RelatorioSessoesPorPeriodoQuery`
- [x] Query: `RelatorioRepassesMensaisQuery`
- [x] Query: `RelatorioInadimplenciaQuery`

### 5.3 — Frontend Dashboard
- [x] Tela de Dashboard principal com cards de KPI no Angular
- [x] Gráfico de fluxo de caixa (linha) — previsto vs realizado
- [x] Gráfico de sessões por status (pizza/barra)
- [x] Gráfico de absenteísmo por psicólogo
- [x] Tabela de pacientes inadimplentes
- [x] Filtros de período (semana/mês/trimestre/custom)
- [x] Testes de queries de KPI

---

## MÓDULO 6 — Emissão de Documentos

### 6.1 — Recibos PDF
- [x] Instalar biblioteca de geração PDF (QuestPDF ou similar)
- [x] Template de recibo (dados clínica, paciente, sessão, valor, forma pgto)
- [x] Command: `EmitirReciboCommand` + Handler
- [x] Endpoint: `GET /recibos/{sessaoId}` (retorna PDF)
- [x] Armazenamento do PDF gerado (S3/local)

### 6.2 — NFSe (Nota Fiscal de Serviço Eletrônica)
- [x] Pesquisar e configurar API de NFSe (prefeitura da cidade cadastrada)
- [x] Command: `EmitirNFSeCommand` + Handler
- [x] Criar entidade `NotaFiscal` (número, XML, status, link)
- [x] Endpoint: `POST /notas-fiscais`
- [x] Tela de emissão de recibos/notas no Angular

### 6.3 — Relatórios para Envio
- [x] Gerar relatório mensal por psicólogo em PDF
- [ ] Command: `EnviarRelatorioMensalCommand` → disparar webhook N8N (WhatsApp/email)
- [x] Tela de relatórios com histórico de envios no Angular
- [ ] Testes de geração de documentos

---

## MÓDULO 7 — Relatórios Personalizados (B.I.)

### 7.1 — Motor de Relatórios
- [x] Criar entidade `RelatorioPersonalizado` (nome, descrição, tipo, filtros, criado_por)
- [x] Criar enum `TipoRelatorio` (Financeiro, Sessoes, Pacientes, Psicologos, Inadimplencia, Repasses, FluxoCaixaProjetado, Comparativo)
- [x] Criar enum `FormatoExportacao` (Json, Pdf, Xlsx, Csv)
- [x] Implementar engine de filtros dinâmicos (período, psicólogo, paciente, status, plano de conta)
- [x] Implementar agrupamento configurável (por dia, semana, mês, trimestre, ano)
- [x] Implementar ordenação nos resultados
- [x] Command: `CriarRelatorioPersonalizadoCommand` + Handler + Validator
- [x] Command: `AtualizarRelatorioPersonalizadoCommand` + Handler + Validator
- [x] Command: `ExcluirRelatorioPersonalizadoCommand` + Handler (soft delete)
- [x] Command: `MarcarFavoritoCommand` + Handler
- [x] Query: `ListarRelatoriosPersonalizadosQuery` + Handler
- [x] Query: `ObterRelatorioPersonalizadoQuery` + Handler
- [x] Query: `ExecutarRelatorioQuery` + Handler (8 tipos de relatório implementados)

### 7.2 — Templates de Relatório Pré-definidos
- [x] Template: Faturamento por período (receitas vs despesas)
- [x] Template: Produtividade por psicólogo (sessões realizadas, faltas, receita gerada)
- [x] Template: Análise de inadimplência (aging report — 30/60/90 dias)
- [x] Template: Comparativo mensal (mês atual vs anterior vs mesmo mês ano anterior)
- [x] Template: Ranking de pacientes por receita gerada
- [ ] Template: Ocupação de agenda (horários ocupados vs disponíveis)
- [x] Template: Repasses por psicólogo com detalhamento por sessão
- [x] Template: Fluxo de caixa projetado (próximos 30/60/90 dias)

### 7.3 — Exportação e Visualização
- [x] Exportar relatórios em PDF (QuestPDF)
- [x] Exportar relatórios em Excel (.xlsx — ClosedXML)
- [x] Exportar relatórios em CSV (implementação manual)
- [x] Endpoint: `GET /relatorios-bi/{id}/executar` (JSON com dados)
- [x] Endpoint: `POST /relatorios-bi/executar` (ad-hoc, sem salvar)
- [x] Endpoint: `GET /relatorios-bi/{id}/exportar?formato=pdf|xlsx|csv`
- [x] Endpoint: `POST /relatorios-bi/exportar` (ad-hoc)
- [x] Endpoints REST: CRUD completo `/relatorios-bi`
- [x] Endpoint: `GET /relatorios-bi/templates`
- [x] Endpoint: `PATCH /relatorios-bi/{id}/favorito`
- [x] Migration EF Core: tabela `relatorios_personalizados`
- [x] Serviço `RelatorioExportService` registrado na Infrastructure DI

### 7.4 — Frontend B.I.
- [x] Tela de listagem de relatórios (pré-definidos + personalizados) com abas
- [x] Filtro por tipo de relatório na listagem
- [x] Cards para templates com ícones por tipo
- [x] Botões "Executar" e "Personalizar" nos templates
- [x] Tela de criação/edição de relatório com construtor visual de filtros
- [x] Filtros dinâmicos por tipo de relatório no editor
- [x] Preview ("Testar Filtros") no editor
- [x] Tela de visualizador com tabela dinâmica baseada em colunas retornadas
- [x] Visualização em gráfico (bar-chart CSS simplificado + instruções para ng2-charts)
- [x] Toggle de favorito na listagem de relatórios personalizados
- [x] Botões de exportação (PDF, XLSX, CSV) com download via Blob URL
- [x] Rota `relatorios-bi` adicionada ao `app.routes.ts`
- [x] Item "Relatórios B.I." adicionado ao sidebar
- [ ] Drill-down: clicar em dado do gráfico para ver detalhes
- [ ] Agendar envio automático de relatório por email/WhatsApp (via N8N)
- [ ] Testes unitários e de integração

---

## FASE FINAL — Qualidade & Deploy

### Segurança
- [ ] Auditoria de segurança: verificar injeção SQL, XSS, IDOR
- [ ] Confirmar criptografia de dados LGPD em todos os campos sensíveis
- [ ] Implementar suporte à portabilidade e exclusão de dados (LGPD)
- [ ] Revisar rate limiting e proteção de endpoints públicos
- [ ] Testes de penetração básicos (endpoints de autenticação)

### Testes & Qualidade
- [ ] Cobertura Domain ≥ 90%
- [ ] Cobertura Application ≥ 80%
- [ ] Cobertura Infrastructure ≥ 60%
- [ ] Testes end-to-end dos fluxos críticos (contrato → sessão → lançamento → fechamento)
- [ ] Testes de carga básicos (tempo de resposta <2s)

### Infraestrutura & Deploy
- [ ] Configurar CI/CD (GitHub Actions ou similar)
- [ ] Configurar ambiente de staging
- [ ] Configurar ambiente de produção
- [ ] Configurar backup automatizado diário (retenção 90 dias)
- [ ] Configurar monitoramento e alertas de uptime
- [ ] Configurar migrations automáticas no deploy
- [ ] Documentar variáveis de ambiente necessárias

### Onboarding
- [ ] Implementar checklist de onboarding interativo no Angular
- [ ] Criar tooltips e guias in-app para novos usuários
- [ ] Validar fluxo completo em <30 minutos
- [ ] Criar dados de seed/demo para novos tenants

---

## Progresso Geral

| Módulo | Total Tarefas | Concluídas | % |
|---|---|---|---|
| Fase 0 — Setup | 21 | 21 | 100% |
| Fase 1 — Auth & Tenancy | 17 | 17 | 100% |
| Módulo 1 — Cadastros | 45 | 45 | 100% |
| Módulo 2 — Sessões | 20 | 20 | 100% |
| Módulo 3 — Financeiro | 26 | 26 | 100% |
| Módulo 4 — Automações N8N | 16 | 0 | 0% |
| Módulo 5 — Dashboard | 16 | 16 | 100% |
| Módulo 6 — Documentos | 14 | 12 | 86% |
| Módulo 7 — Relatórios B.I. | 32 | 28 | 88% |
| Fase Final — Qualidade & Deploy | 19 | 0 | 0% |
| **TOTAL** | **226** | **185** | **82%** |

---

*Atualizado em: 2026-04-02 — Módulo 7 implementado*
