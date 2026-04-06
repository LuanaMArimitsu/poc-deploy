using FluentValidation;
using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Validators.Comunicacao
{
    public class MensagemRequestValidator : AbstractValidator<MensagemRequestDTO>
    {
        private readonly string[] tiposPermitidos = ["text", "image", "sticker", "audio", "document", "video"];

        public MensagemRequestValidator()
        {
            RuleFor(x => x.TipoMensagem)
                .NotEmpty().WithMessage("Tipo de mensagem é obrigatório.")
                .Must(t => tiposPermitidos.Contains(t.ToLower()))
                .WithMessage("Tipo de mensagem inválido. Valores permitidos: text, image, sticker, audio, document, video.");

            // Midia e Template não podem ser ambos verdadeiros
            RuleFor(x => x)
                .Must(x => !(x.Midia && x.Template))
                .WithMessage("Uma mensagem não pode ser mídia e template ao mesmo tempo.");

            RuleFor(x => x.LeadId)
                .GreaterThan(0).WithMessage("LeadId deve ser maior que zero.");

            RuleFor(x => x.UsuarioId)
                .GreaterThan(0).WithMessage("UsuarioId deve ser maior que zero.");

            When(x => x.Midia, () =>
            {
                RuleFor(x => x.TipoMensagem.ToLower())
                    .NotEqual("text").WithMessage("Mensagens com mídia não podem ter tipo 'text'.");

                RuleFor(x => x.File)
                    .NotNull().WithMessage("Arquivo é obrigatório para mensagens com mídia.")
                    .Must(f => f != null && f.Length > 0)
                    .WithMessage("Arquivo não pode estar vazio.");

                RuleFor(x => x)
                    .Must(x =>
                    {
                        var contentType = x.File?.ContentType ?? "";
                        return !contentType.StartsWith("audio") || string.IsNullOrWhiteSpace(x.Conteudo);
                    })
                    .WithMessage("Mensagens com áudio não podem conter conteúdo textual.");
            });


            When(x => x.Template, () =>
            {
                RuleFor(x => x.TemplateId)
                    .NotNull().WithMessage("TemplateId é obrigatório quando 'Template' for verdadeiro.");

                RuleFor(x => x.Conteudo)
                    .Must(string.IsNullOrWhiteSpace)
                    .WithMessage("Mensagens de template não devem conter conteúdo.");

                RuleFor(x => x.TipoMensagem.ToLower())
                     .Equal("text").WithMessage("Mensagens de template devem ter tipo 'text'.");
            });

            When(x => !x.Midia && !x.Template, () =>
            {
                RuleFor(x => x.Conteudo)
                    .NotEmpty().WithMessage("Conteúdo é obrigatório quando não for mídia nem template.");
            });

            RuleFor(x => x)
            .Must(x => x.File == null || x.Midia)
            .WithMessage("Se um arquivo for enviado, 'Midia' deve ser verdadeiro.");

            RuleFor(x => x.TemplateId)
            .Must((dto, templateId) =>
                !templateId.HasValue || dto.Template)
            .WithMessage("TemplateId só pode ser preenchido se 'Template' for verdadeiro.");

        }
    }
}
