# Modelos RDL externos

Estes 20 ficheiros são os modelos usados pelo viewer e pela exportação PDF.
São RDL XML com o namespace `http://schemas.microsoft.com/sqlserver/reporting/2005/01/reportdefinition`,
compatível com Majorsilence Reporting. Não contêm dados reais nem credenciais.

## Carregamento e edição

Na compilação e publicação, os ficheiros são copiados para `Reports/`, junto ao
executável. A aplicação resolve a pasta a partir de `AppContext.BaseDirectory`,
independentemente da pasta de trabalho. Cada abertura lê novamente o respetivo
RDL; não é necessário recompilar nem reiniciar para aplicar uma alteração ao
layout. Feche e reabra o relatório. Uma janela já aberta mantém o modelo e os
dados carregados, incluindo na exportação PDF.

Edite os ficheiros XML ou use um editor RDL compatível. Pode alterar fontes,
cores, posições, larguras de colunas e orientação. Mantenha os nomes dos
parâmetros, datasets e campos. As larguras de cada `TableColumn` estão definidas
no próprio ficheiro e devem somar a largura da tabela e caber na página, depois
de descontadas as margens. `CanGrow` permite a quebra de texto e os cabeçalhos de
grupo repetem-se nas páginas seguintes. O título usa Arial 14 pt; as tabelas usam
Arial 8 pt. O idioma é `pt-PT`.

Para guardar alterações no repositório, edite esta pasta de origem. Uma nova
compilação/publicação pode substituir as cópias na pasta de execução.
Ficheiros ausentes ou inválidos apresentam um erro em português com o caminho
do modelo; não existe geração automática de um RDL alternativo.

## Correspondência

O nome sem extensão corresponde ao identificador `ReportTemplate`. Esta seleção
é explícita e não depende do nome do ficheiro Excel.

| Ficheiro | Relatório | Datasets e número de colunas | Página |
| --- | --- | --- | --- |
| Clientes.rdl | Clientes | Data0: 7 | A4 horizontal |
| Fornecedores.rdl | Fornecedores | Data0: 7 | A4 horizontal |
| Produtos.rdl | Produtos | Data0: 7 | A4 horizontal |
| Impostos.rdl | Tabela de impostos | Data0: 7 | A4 horizontal |
| ErrosValidacao.rdl | Erros de validação | Data0: 3 | A4 vertical |
| Inventario.rdl | Existências | Data0: 6 | A4 vertical |
| LancamentosContabilisticos.rdl | Diários e lançamentos contabilísticos | Data0: 11 | A4 horizontal |
| FaturacaoDocumentos.rdl | Lista de documentos de faturação e detalhes | Data0: 11; Data1: 15 | A4 horizontal |
| FaturacaoImpostos.rdl | Impostos de faturação | Data0: 4 | A4 vertical |
| FaturacaoDocumento.rdl | Documento de faturação selecionado | Data0: 15 | A4 horizontal |
| MercadoriasDocumentos.rdl | Documentos de movimentação de mercadorias | Data0: 14 | A4 horizontal |
| MercadoriasImpostos.rdl | Impostos de movimentação de mercadorias | Data0: 4 | A4 vertical |
| MercadoriasDocumento.rdl | Documento de movimentação selecionado | Data0: 14 | A4 horizontal |
| ConferenciaDocumentos.rdl | Documentos de conferência | Data0: 17 | A4 horizontal |
| ConferenciaImpostos.rdl | Impostos de documentos de conferência | Data0: 4 | A4 vertical |
| ConferenciaDocumento.rdl | Documento de conferência selecionado | Data0: 17 | A4 horizontal |
| PagamentosDocumentos.rdl | Documentos de pagamento | Data0: 11 | A4 horizontal |
| PagamentosImpostos.rdl | Impostos de pagamentos | Data0: 4 | A4 vertical |
| PagamentosDocumento.rdl | Pagamento selecionado | Data0: 11 | A4 horizontal |
| PagamentosLinhas.rdl | Linhas de pagamento filtradas | Data0: 9 | A4 horizontal |

## Parâmetros e dados em memória

- `Titulo` (`String`): título fornecido pela aplicação.
- `Empresa` (`String`, pode estar vazio): empresa e NIF, ou NIF do inventário.
- Os parâmetros não têm `Prompt`, pelo que não são pedidos ao utilizador.
- `=Parameters!Titulo.Value` e `=Parameters!Empresa.Value` são usados no cabeçalho.
- O rodapé usa `Globals!PageNumber` e `Globals!TotalPages`.

A aplicação fornece estes parâmetros e também fixa os valores predefinidos da
instância carregada, sem alterar o ficheiro em disco. Isto preserva título e
empresa quando o motor limpa a cache entre a pré-visualização e a exportação.

Os dados são fornecidos com `DataSets["DataN"].SetData(DataTable)` antes de executar
o relatório. A fonte `Memory` é uma declaração RDL sem ligação configurada; não é
executada uma consulta SQL. Os datasets têm esquemas fixos, incluindo tabelas
vazias e colunas opcionais sem valores. `Data0` corresponde à primeira folha;
apenas `FaturacaoDocumentos` tem `Data1`, correspondente à folha de detalhes.

Cada dataset contém:

| Campo | Tipo .NET | Utilização |
| --- | --- | --- |
| Block | Int32 | Identificador de grupo, pela ordem dos documentos e secções |
| C0 … C(N-1) | String | Valores apresentados, já formatados em português |
| H0 … H(N-1) | String | Cabeçalhos correspondentes aos valores de cada grupo |

Os índices começam em zero. `N` é o número de colunas indicado na tabela acima.
Por exemplo, em `Clientes`, `C0` é o NIF e `C1` é o nome; `H0` e `H1` contêm os
respetivos cabeçalhos. Os campos `C` são texto de apresentação; os cálculos e
totais são preparados pela aplicação. Datas usam `dd/MM/yyyy`; números usam a
formatação `pt-PT`. Valores ausentes são strings vazias, sem remoção de campos.

Nos relatórios com documentos e linhas na mesma tabela, `Block` separa as
secções e os campos `H` mudam para os cabeçalhos dos detalhes. Preserve o
agrupamento por `=Fields!Block.Value` e as expressões `=Fields!H0.Value`, etc.,
para repetir o cabeçalho correto em cada página.

Ordem das colunas nas listas:

- Clientes/fornecedores: NIF, nome, ID, morada, telefone, fax, email.
- Produtos: código, descrição, preço, código de barras, grupo, IVA, preço com IVA.
- Impostos: tipo, descrição, código, país/região, montante, percentagem, validade.
- Erros: campo, valor, descrição.
- Inventário: categoria, código, descrição, código de barras, quantidade, unidade.
- Resumos de impostos: documento, imposto, incidência, total.

O contrato de dados é independente das exportações XLSX/CSV, que preservam os
dados e fórmulas existentes. Alterar um layout RDL afeta a visualização e o PDF.
