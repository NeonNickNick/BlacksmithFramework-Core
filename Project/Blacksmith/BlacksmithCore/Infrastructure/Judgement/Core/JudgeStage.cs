using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;

namespace BlacksmithCore.Infrastructure.Judgement.Core
{
    public partial class JudgeStage : BlacksmithEnum<JudgeStage>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue OnBegin() => _OnBegin_GetOrCreate();
        [IsBlacksmithEnumMember(8)]
        public CEValue OnApplyingEffect() => _OnApplyingEffect_GetOrCreate();
        [IsBlacksmithEnumMember(16)]
        public CEValue OnEffectTaking_AfterAnalyzableDataWritten() => _OnEffectTaking_AfterAnalyzableDataWritten_GetOrCreate();
        [IsBlacksmithEnumMember(32)]
        public CEValue OnAttackCanceling() => _OnAttackCanceling_GetOrCreate();

        [IsBlacksmithEnumMember(64)]
        public CEValue OnEffectTaking_AfterTransport() => _OnEffectTaking_AfterTransport_GetOrCreate();
        [IsBlacksmithEnumMember(128)]
        public CEValue OnApplyingOthers() => _OnApplyingOthers_GetOrCreate();
        [IsBlacksmithEnumMember(256)]
        public CEValue OnUpdating() => _OnUpdating_GetOrCreate();
        [IsBlacksmithEnumMember(1024)]
        public CEValue OnEffectTaking_AfterResult() => _OnEffectTaking_AfterResult_GetOrCreate();
        [IsBlacksmithEnumMember(2048)]
        public CEValue OnEnd() => _OnEnd_GetOrCreate();
    }
}
