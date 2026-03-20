# CHECKLIST — Sistema de Gestão para Clínicas de Psicologia

> Marque `[x]` ao concluir cada tarefa. Atualize este arquivo a cada sessão de desenvolvimento.
> Stack: .NET Core 8 | Angular 18+ | PostgreSQL 16 | N8N

---

## FASE 0 — Infraestrutura & Setup Inicial

### 0.1 — Estrutura do Projeto
- [x] Criar solution .NET com estrutura de pastas (Api, Domain, Application, Infrastructure, Tests)
- [ ] Configurar projeto Angular 18+ com lazy-loaded modules
- [x] Criar scripts SQL para todas as tabelas (pasta `/sql`) — Supabase + N8N já hospedados na VPS
- [x] Configurar `.gitignore`, `.editorconfig` e `README.md`

### 0.2 — Configurações Base .NET
- [x] Instalar pacotes NuGet: MediatR, FluentValidation, EF Core, Hangfire, JWT Bearer
- [x] Instalar pacotes NuGet: xUnit, FluentAssertions, Testcontainers
- [ ] Configurar `appsettings.json` e `appsettings.Development.json`
- [ ] Configurar Serilog para logging estruturado
- [ ] Configurar ProblemDetails (RFC 7807) para respostas de erro
- [ ] Configurar rate limiting (100 req/min público, 300 req/min autenticado)
- [ ] Configurar CORS para o frontend Angular

### 0.3 — Configurações Base Angular
- [ ] Configurar `ApiService` centralizado (HttpClient)
- [ ] Configurar interceptor de JWT (Bearer token)
- [ ] Configurar interceptor de refresh token (cookie HttpOnly)
- [ ] Configurar NgRx Signal Store para estado global
- [ ] Configurar estratégia OnPush em todos os componentes
- [ ] Configurar guards de rota (autenticação)
- [ ] Configurar tratamento global de erros HTTP

### 0.4 — Banco de Dados & Multi-tenancy
- [ ] Configurar EF Core com PostgreSQL (snake_case naming)
- [ ] Implementar estratégia multi-tenant: schema por tenant
- [ ] Criar `TenantMiddleware` para resolver tenant via JWT/subdomínio
- [ ] Implementar `TenantDbContext` com filtros globais por tenant
- [ ] Criar mecanismo de criação automática de schema por novo tenant
- [ ] Configurar soft delete global (`excluido_em`)
- [ ] Configurar TIMESTAMPTZ para todos os campos de data

---

## FASE 1 — Autenticação & Multi-tenancy

### 1.1 — Autenticação (Auth)
- [ ] Criar entidade `Usuario` com roles (Admin, Gerente, Secretaria, Psicologo)
- [ ] Implementar registro de usuário com hash de senha (BCrypt)
- [ ] Implementar login com geração de JWT (access 15min + refresh 7d)
- [ ] Implementar refresh token via cookie HttpOnly
- [ ] Implementar logout (invalidar refresh token)
- [ ] Implementar troca de senha
- [ ] Implementar recuperação de senha (email)
- [ ] Criar tela de Login no Angular
- [ ] Criar tela de Recuperação de Senha no Angular
- [ ] Testes unitários de autenticação (90% cobertura domain)

### 1.2 — Gestão de Tenants (Clínicas)
- [ ] Criar entidade `Clinica` (tenant raiz)
- [ ] Implementar criação de novo tenant (schema PostgreSQL)
- [ ] Implementar isolamento de dados por tenant em todas as queries
- [ ] Criar endpoint de onboarding inicial (setup da clínica)
- [ ] Criar checklist de onboarding no Angular (<30 min meta)
- [ ] Implementar auditoria: log de todas as ações financeiras
- [ ] Testes de isolamento multi-tenant

---

## MÓDULO 1 — Cadastros

### 1.1 — Clínica
- [ ] Criar entidade `Clinica` (nome, CNPJ, endereço, telefone, email)
- [ ] Command: `CriarClinicaCommand` + Handler + Validator
- [ ] Command: `AtualizarClinicaCommand` + Handler + Validator
- [ ] Query: `ObterClinicaQuery` + Handler
- [ ] Endpoint REST: `POST /clinicas`, `PUT /clinicas/{id}`, `GET /clinicas/{id}`
- [ ] Tela de cadastro/edição de clínica no Angular
- [ ] Testes unitários e de integração

### 1.2 — Psicólogos
- [ ] Criar entidade `Psicologo` (nome, CRP, tipo PJ/CLT, percentual ou valor fixo repasse)
- [ ] Command: `CriarPsicologoCommand` + Handler + Validator
- [ ] Command: `AtualizarPsicologoCommand` + Handler + Validator
- [ ] Command: `InativarPsicologoCommand` + Handler
- [ ] Query: `ListarPsicologosQuery` + Handler (com filtros)
- [ ] Query: `ObterPsicologoQuery` + Handler
- [ ] Endpoints REST: CRUD completo `/psicologos`
- [ ] Tela de listagem de psicólogos no Angular
- [ ] Tela de cadastro/edição de psicólogo no Angular
- [ ] Testes unitários e de integração

### 1.3 — Pacientes
- [ ] Criar entidade `Paciente` (nome, CPF, email, telefone, data nascimento)
- [ ] Implementar criptografia de dados sensíveis (LGPD)
- [ ] Command: `CriarPacienteCommand` + Handler + Validator
- [ ] Command: `AtualizarPacienteCommand` + Handler + Validator
- [ ] Command: `InativarPacienteCommand` + Handler
- [ ] Query: `ListarPacientesQuery` + Handler (com busca/filtros)
- [ ] Query: `ObterPacienteQuery` + Handler
- [ ] Endpoints REST: CRUD completo `/pacientes`
- [ ] Tela de listagem de pacientes no Angular
- [ ] Tela de cadastro/edição de paciente no Angular
- [ ] Testes unitários e de integração

### 1.4 — Contratos
- [ ] Criar entidade `Contrato` (paciente, psicólogo, valor sessão, dia semana, frequência, forma pgto)
- [ ] Criar enum `StatusContrato` (Ativo, Pausado, Encerrado)
- [ ] Criar enum `FormaPagamento` (PIX, Cartão, Dinheiro, Convênio)
- [ ] Criar enum `FrequenciaContrato` (Semanal, Quinzenal)
- [ ] Command: `CriarContratoCommand` + Handler + Validator
- [ ] Command: `AtualizarContratoCommand` + Handler + Validator
- [ ] Command: `EncerrarContratoCommand` + Handler
- [ ] Query: `ListarContratosQuery` + Handler
- [ ] Query: `ObterContratoQuery` + Handler
- [ ] Endpoints REST: CRUD completo `/contratos`
- [ ] Tela de listagem de contratos no Angular
- [ ] Tela de cadastro/edição de contrato no Angular
- [ ] Testes unitários e de integração

### 1.5 — Planos de Conta (Financeiro)
- [ ] Criar entidade `PlanoConta` (nome, tipo Receita/Despesa, categoria)
- [ ] CRUD completo + endpoints REST
- [ ] Tela de gestão de planos de conta no Angular
- [ ] Testes unitários

---

## MÓDULO 2 — Sessões

### 2.1 — Agendamento de Sessões
- [ ] Criar entidade `Sessao` (contrato, data, hora, psicólogo, paciente, status)
- [ ] Criar enum `StatusSessao` (Agendada, Realizada, Falta, FaltaJustificada, Cancelada)
- [ ] Regra: sessão só pode mudar status dentro de 30 dias (exceto Admin)
- [ ] Command: `AgendarSessaoCommand` + Handler + Validator
- [ ] Command: `AtualizarSessaoCommand` + Handler + Validator
- [ ] Command: `CancelarSessaoCommand` + Handler

### 2.2 — Recorrência Semanal/Quinzenal
- [ ] Implementar `GerarSessoesRecorrentesCommand` (a partir do contrato)
- [ ] Serviço de domínio: calcular próximas N sessões com base na frequência
- [ ] Hangfire job: gerar sessões do mês seguinte automaticamente
- [ ] Regra: ao gerar sessão, criar lançamento financeiro com status `Previsto`

### 2.3 — Controle de Frequência
- [ ] Command: `MarcarPresencaCommand` + Handler
- [ ] Command: `RegistrarFaltaCommand` (justificada/não justificada) + Handler
- [ ] Regra de negócio: falta justificada não gera cobrança (configurável por contrato)
- [ ] Query: `ListarSessoesQuery` + Handler (filtros: data, psicólogo, paciente, status)
- [ ] Query: `ObterSessaoQuery` + Handler

### 2.4 — Endpoints & Frontend
- [ ] Endpoints REST: `/sessoes` CRUD completo
- [ ] Endpoint: `POST /sessoes/gerar-recorrentes`
- [ ] Tela de agenda/calendário de sessões no Angular
- [ ] Tela de listagem de sessões com filtros no Angular
- [ ] Marcação de presença/falta inline na listagem
- [ ] Testes unitários e de integração (regras de recorrência e status)

---

## MÓDULO 3 — Financeiro

### 3.1 — Lançamentos Financeiros
- [ ] Criar entidade `LancamentoFinanceiro` (descrição, valor, tipo, status, vencimento, competência)
- [ ] Criar enum `TipoLancamento` (Receita, Despesa)
- [ ] Criar enum `StatusLancamento` (Previsto, Confirmado, Cancelado)
- [ ] Criar Domain Event: `SessaoRealizadaEvent` → confirmar lançamento
- [ ] Criar Domain Event: `SessaoCanceladaEvent` → cancelar lançamento
- [ ] Command: `CriarLancamentoCommand` + Handler + Validator
- [ ] Command: `AtualizarLancamentoCommand` + Handler + Validator
- [ ] Command: `ConfirmarPagamentoCommand` + Handler
- [ ] Command: `CancelarLancamentoCommand` + Handler
- [ ] Query: `ListarLancamentosQuery` + Handler (filtros: período, tipo, status)
- [ ] Query: `ObterFluxoCaixaQuery` (diário/semanal/mensal, previsto vs realizado)

### 3.2 — Repasses para Psicólogos PJ
- [ ] Serviço de domínio: `CalcularRepasseService` (percentual ou valor fixo)
- [ ] Command: `GerarRepasseMensalCommand` + Handler
- [ ] Criar entidade `Repasse` (psicólogo, mês, valor calculado, status)
- [ ] Endpoint: `POST /repasses/calcular`
- [ ] Query: `ListarRepassesQuery` + Handler

### 3.3 — Fechamento Mensal
- [ ] Command: `RealizarFechamentoMensalCommand` + Handler
- [ ] Regra: período fechado não permite edições de lançamentos
- [ ] Criar entidade `FechamentoMensal` (mês, totais, status)
- [ ] Gerar relatório consolidado por psicólogo no fechamento
- [ ] Endpoint: `POST /fechamentos`
- [ ] Query: `ObterFechamentoQuery` + Handler

### 3.4 — Endpoints & Frontend
- [ ] Endpoints REST: `/lancamentos` CRUD completo
- [ ] Endpoints REST: `/repasses`, `/fechamentos`
- [ ] Tela de lançamentos financeiros com filtros no Angular
- [ ] Tela de fluxo de caixa (gráficos diário/mensal) no Angular
- [ ] Tela de repasses por psicólogo no Angular
- [ ] Tela de fechamento mensal no Angular
- [ ] Testes unitários e de integração (regras de fechamento, repasse)

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
- [ ] Query: `ObterKpisDashboardQuery` (período configurável)
- [ ] KPI: taxa de absenteísmo (faltas / sessões agendadas)
- [ ] KPI: taxa de inadimplência (previsto não recebido / previsto total)
- [ ] KPI: ticket médio por sessão
- [ ] KPI: receita realizada vs projetada (mensal)
- [ ] KPI: sessões realizadas vs agendadas
- [ ] KPI: ranking de psicólogos por volume de sessões/receita

### 5.2 — Relatórios
- [ ] Query: `RelatorioFluxoCaixaQuery` (exportável)
- [ ] Query: `RelatorioSessoesPorPeriodoQuery`
- [ ] Query: `RelatorioRepassesMensaisQuery`
- [ ] Query: `RelatorioInadimplenciaQuery`

### 5.3 — Frontend Dashboard
- [ ] Tela de Dashboard principal com cards de KPI no Angular
- [ ] Gráfico de fluxo de caixa (linha) — previsto vs realizado
- [ ] Gráfico de sessões por status (pizza/barra)
- [ ] Gráfico de absenteísmo por psicólogo
- [ ] Tabela de pacientes inadimplentes
- [ ] Filtros de período (semana/mês/trimestre/custom)
- [ ] Testes de queries de KPI

---

## MÓDULO 6 — Emissão de Documentos

### 6.1 — Recibos PDF
- [ ] Instalar biblioteca de geração PDF (QuestPDF ou similar)
- [ ] Template de recibo (dados clínica, paciente, sessão, valor, forma pgto)
- [ ] Command: `EmitirReciboCommand` + Handler
- [ ] Endpoint: `GET /recibos/{sessaoId}` (retorna PDF)
- [ ] Armazenamento do PDF gerado (S3/local)

### 6.2 — NFSe (Nota Fiscal de Serviço Eletrônica)
- [ ] Pesquisar e configurar API de NFSe (prefeitura da cidade cadastrada)
- [ ] Command: `EmitirNFSeCommand` + Handler
- [ ] Criar entidade `NotaFiscal` (número, XML, status, link)
- [ ] Endpoint: `POST /notas-fiscais`
- [ ] Tela de emissão de recibos/notas no Angular

### 6.3 — Relatórios para Envio
- [ ] Gerar relatório mensal por psicólogo em PDF
- [ ] Command: `EnviarRelatorioMensalCommand` → disparar webhook N8N (WhatsApp/email)
- [ ] Tela de relatórios com histórico de envios no Angular
- [ ] Testes de geração de documentos

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
| Fase 0 — Setup | 21 | 0 | 0% |
| Fase 1 — Auth & Tenancy | 16 | 0 | 0% |
| Módulo 1 — Cadastros | 45 | 0 | 0% |
| Módulo 2 — Sessões | 20 | 0 | 0% |
| Módulo 3 — Financeiro | 24 | 0 | 0% |
| Módulo 4 — Automações N8N | 16 | 0 | 0% |
| Módulo 5 — Dashboard | 16 | 0 | 0% |
| Módulo 6 — Documentos | 14 | 0 | 0% |
| Fase Final — Qualidade & Deploy | 19 | 0 | 0% |
| **TOTAL** | **191** | **0** | **0%** |

---

*Atualizado em: 2026-03-20*
