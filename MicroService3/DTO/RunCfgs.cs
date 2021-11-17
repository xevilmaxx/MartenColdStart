using System.ComponentModel.DataAnnotations;

namespace MicroService3.DTO
{
    public static class RunCfgs
    {
        [Display(Name = "listening_port", Description = "Own Server Port Number [0 => let choose to GRPC]")]
        public static int OwnServerPort { get; set; } = 30033;

        public static string Addresses { get; set; } = "";

        public static string DisabledInputs { get; set; } = "";

        [Display(AutoGenerateField = false)]
        public static bool IsNeedUseSSL { get; set; } = false;

        [Display(Description = "Determines if Microservice is queriable from external PC")]
        public static bool IsPubliclyVisibleService { get; set; } = true;

    }
}
