# Monitoramento Urbano

> Sistema de **Monitoramento Urbano** desenvolvido para prefeituras, com cadastro e gerenciamento de **câmeras** e **vídeos** vinculados.
>
> O projeto permite:
>
> * Registrar câmeras de monitoramento (descrição, coordenadas geográficas, FPS).
> * Cadastrar vídeos associados a uma câmera específica (nome do arquivo, caminho/URL, data e horário de início, usuário responsável).
> * Relacionar vídeos diretamente às câmeras cadastradas.
> * Visualizar, pesquisar e gerenciar registros de forma simples.
>
> 🔗 Objetivo: apoiar a gestão municipal em projetos de **cidades inteligentes** e **segurança urbana**.

---

## ✅ Checklist do Projeto

- [x] Criada entidade **Camera**
- [x] Criada entidade **Video**
- [x] Criado **IRepositorio**, **ICameraRepositorio** e **IVideoRepositorio**
- [x] Implementados **CameraRepositorio** e **VideoRepositorio** usando **PostgreSQL + Dapper**
- [x] Ajustada inserção no PostgreSQL usando `RETURNING Id`
- [x] Criada camada de **Controller** (`CameraController`, `MonitoramentoController`)
- [x] Criadas **Views parciais** (`_Form`, `_List`, `_Tab`) para Câmeras e Vídeos
- [x] Aplicado **TailwindCSS** na estilização (layout com tabs, cards e navbar clean)
- [ ] Implementar validações de negócio avançadas (`data_upload`, `horario_inicio`)
- [ ] Criar alertas customizados (feedback visual via Tailwind)
- [ ] Testes unitários de repositórios e controllers
- [ ] Deploy do sistema para ambiente de homologação

---

## 🚀 Tecnologias Utilizadas

<p align="left">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
  <img src="https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/PostgreSQL-336791?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/Dapper-512BD4?style=for-the-badge&logo=nuget&logoColor=white" />
  <img src="https://img.shields.io/badge/Razor-000000?style=for-the-badge&logo=razorpay&logoColor=white" />
  <img src="https://img.shields.io/badge/TailwindCSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white" />
</p>

---

## 👥 Colaboradores

- [Fábio Henrique](https://github.com/FabioHenrique023)
- [Tayssa Victoria](https://github.com/tayxvv)