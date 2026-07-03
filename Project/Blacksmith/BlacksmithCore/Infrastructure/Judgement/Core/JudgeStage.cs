using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;

namespace BlacksmithCore.Infrastructure.Judgement.Core
{
    public class JudgeStage : BlacksmithEnum<JudgeStage>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue OnBegin() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue OnApplyingEffect() => GetCEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue OnEffectTaking_AfterAnalyzableDataWritten() => GetCEValue();
        [IsBlacksmithEnumMember(32)]
        public CEValue OnAttackCanceling() => GetCEValue();

        [IsBlacksmithEnumMember(64)]
        public CEValue OnEffectTaking_AfterTransport() => GetCEValue();
        [IsBlacksmithEnumMember(128)]
        public CEValue OnApplyingOthers() => GetCEValue();
        [IsBlacksmithEnumMember(256)]
        public CEValue OnUpdating() => GetCEValue();
        [IsBlacksmithEnumMember(1024)]
        public CEValue OnEffectTaking_AfterResult() => GetCEValue();
        [IsBlacksmithEnumMember(2048)]
        public CEValue OnEnd() => GetCEValue();
    }
}
