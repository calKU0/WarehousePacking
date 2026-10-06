using System.Text;
using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Server.Helpers
{
    /// <summary>
    /// Builds the extra "by plane" label printed right after the courier label
    /// of an air shipment, so these packages stand out on the dispatch floor.
    /// </summary>
    public static class PlaneLabelHelper
    {
        // 4" x 6" @ 203 dpi — the same media the courier labels are printed on.
        private const int LabelWidth = 812;
        private const int LabelHeight = 1218;

        // 440 x 440 px plane silhouette as a ZPL ASCII-compressed ^GF field
        // (55 bytes per row, 24200 bytes in total).
        private const int PlaneSize = 440;
        private const int PlaneBytesPerRow = 55;
        private const int PlaneTotalBytes = 24200;
        private const string PlaneGraphic =
            ",:::::::::::::::::gY01C,gV07IFC,gS0MFE,gP03PF,:gP03PF8,gP03PFC,gP01PFC,gP01PFE,:gQ0QF,gQ0QF8,:gQ0QFC,gQ07PFE,:gQ07QF,gQ03QF8,:gQ03QFC,gQ01QFE,:gQ01RF,:gR0RF8,gR0RFC,:gR07QFE,gR07RF,:gR03RF8,gR03RFC,:gR03RFE,gR01RFE,gR01SF,gR01SF8,gS0SF8,gS0SFC,gS0SFE,gS07RFE,gS07SF,gS07SF8,:gS03SFC,:gS03SFE,gS01TF,:gS01TF8,gT0TFC,:gT0TFE,:gT07TF,gT07TF8,:gT03TFC,gT03TFE,:gT01UF,gT01UF8,:gT01UFC,gU0UFC,gU0UFE,gU0VF,gU07UF,gU07UF8,gU07UFC,gU03UFC,gU03UFE,:gU03VF,gU01VF8,:gU01VFC,gV0VFE,:gV0WF,gV07VF8,:gV07VFC,:gV03VFE,gV03WF,:gV01WF8,gV01WFC,:gW0WFE,gW0XF,:gW0XF8,gW07WF8,gW07WFC,gW07WFE,gW03WFE,gW03XF,gW03XF8,gW01XF8,gW01XFC,gW01XFE,:gX0YF,:gX0YF8,gX07XFC,:gX07XFE,gX07YF,gX03YF,gX03YF8,gX03YFC,gX01YFC,gX01YFE,:gY0gF,gY0gF8,:gY0gFC,gY07YFE,:gY07gF,gY03gF,gY03gF8,gY03gFC,:gY01gFE,gY01gGF,:h0gGF8,h0gGFC,:h07gFE,:h07gGF,h07gGF8,h03gGF8,h03gGFC,h03gGFE,h01gGFE,PF8gO01gHF,7OFCgO01gHF,7OFCgP0gHF8,3OFEgP0gHFC,3PFgP0gHFC,3PF8gO0gHFE,1PFCgO07gHF,1PFEgO07gHF,0QFgO07gHF8,0QF8gN03gHFC,07PFCgN03gHFC,07PFEgN03gHFE,03QFgN03gHFE,03QFgN01gIF,03QF8gM01gIF8,01QFCgM01gIF8,01QFEgN0gIFC,H0RFgN0gIFE,H0RF8gM0gIFE,H07QFCgM07gIF,H07QFEgM07gIF8,H07RFgM07gIF8,H03RF8gL07gIFC,H03RFCgL03gIFC,H01RFEgL03gIFE,H01RFEgL03gJF,I0SFgL01gJF,I0SF8gK01gJF8,I07RFCgK01gJFC,I07RFEgL0gJFC,I07SFgL0gJFE,I03SF8gK0gKF,I03SFCgK07gJF,I01SFEgK0gKF8,I01kGF8,J0kHF,J0kHFC,J0kIF,J07kHFC,J07kHFE,J03kIF,J03kIF8,J01kIFC,J01kIFE,J01kJF,K0kJF,K0kJF8K07kIF8K07kIFCK03kIFCK03kIFE:K01kIFE:L0kIFEL0kJFL07kIF::::L0kJFL0kIFE:K01kIFE:K03kIFC:K07kIFCK07kIF8:K0kJF,:J01kIFE,J01kIFC,J01kIF8,J03kIF,J03kHFE,J07kHFC,J07kHF8,J0kHFE,J0kHFC,J0kGFE,I01kGF,I01SFEgK07gJFC,I03SFCgK07gJF8,I03SF8gK07gJF,I07SFgL0gKF,I07RFEgL0gJFE,I07RFEgL0gJFC,I0SFCgK01gJFC,I0SF8gK01gJF8,H01SFgL01gJF8,H01RFEgL01gJF,H03RFCgL03gIFE,H03RF8gL03gIFE,H03RFgM03gIFC,H07QFEgM07gIF8,H07QFCgM07gIF8,H0RFCgM07gIF,H0RF8gM0gIFE,01RFgN0gIFE,01QFEgN0gIFC,01QFCgN0gIFC,03QF8gM01gIF8,03QFgN01gIF,07PFEgN01gIF,07PFCgN03gHFE,0QF8gN03gHFC,0QFgO03gHFC,0QFgO07gHF8,1PFEgO07gHF,1PFCgO07gHF,3PF8gO07gGFE,3PFgP0gHFE,7OFEgP0gHFC,7OFCgP0gHF8,PF8gO01gHF8,7OFgP01gHF,h01gGFE,h03gGFE,h03gGFC,h03gGF8,:h07gGF,:h07gFE,h0gGFC,:h0gGF8,h0gGF,gY01gGF,gY01gFE,:gY03gFC,gY03gF8,:gY07gF,gY07YFE,:gY07YFC,gY0gFC,gY0gF8,gY0gF,gX01gF,gX01YFE,gX01YFC,:gX03YF8,gX03YF,:gX07XFE,:gX07XFC,gX0YF8,:gX0YF,gX0XFE,gW01XFE,gW01XFC,:gW03XF8,gW03XF,:gW03WFE,gW07WFC,:gW07WF8,gW0XF,:gW0WFE,gV01WFE,gV01WFC,gV01WF8,:gV03WF,gV03VFE,:gV07VFC,gV07VF8,:gV0WF,:gV0VFE,gV0VFC,gU01VFC,gU01VF8,gU01VF,gU03VF,gU03UFE,gU03UFC,gU07UFC,gU07UF8,:gU07UF,gU0UFE,:gU0UFC,gT01UF8,:gT01UF,gT03UF,gT03TFE,gT03TFC,:gT07TF8,gT07TF,:gT0TFE,:gT0TFC,gS01TF8,:gS01TF,gS01SFE,gS03SFE,gS03SFC,:gS07SF8,gS07SF,:gS0SFE,gS0SFC,:gS0SF8,gR01SF,:gR01RFE,gR03RFE,gR03RFC,gR03RF8,gR07RF8,gR07RF,gR07QFE,:gR0RFC,gR0RF8,:gQ01RF,:gQ01QFE,gQ03QFC,:gQ03QF8,gQ03QF,gQ07QF,gQ07PFE,gQ07PFC,gQ0QFC,gQ0QF8,:gP01QF,gP01PFE,:gP01PFC,gP03PF8,:gP03PF,gP01OFE,gT0LFE,gW03HFC,,::::::::::::::::::";

        public static bool IsRequired(PackageData? package) =>
            package?.ShipmentServices?.ByPlane == true;

        /// <summary>Returns the plane label as base64 ZPL, ready for the print service.</summary>
        public static string BuildBase64(PackageData package)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(BuildZpl(package.Courier.GetDescription())));
        }

        private static string BuildZpl(string courierName)
        {
            const int margin = 20;
            const int frame = 12;
            const int barHeight = 150;
            const int innerWidth = LabelWidth - 2 * margin;

            var name = courierName.ToUpperInvariant().Replace("^", "").Replace("~", "");

            // Font 0 is roughly 0.6 of its height wide for capitals: shrink long
            // names so they stay on a single line inside the name box.
            const int nameBoxWidth = innerWidth - 80;
            var nameFont = Math.Clamp((int)(nameBoxWidth / (Math.Max(name.Length, 1) * 0.6)), 50, 150);

            // Plane and name box are centred vertically in the space below the top bar.
            const int nameBoxHeight = 220;
            const int gap = 40;
            const int contentTop = margin + barHeight;
            const int contentHeight = LabelHeight - margin - contentTop;
            var planeY = contentTop + (contentHeight - (PlaneSize + gap + nameBoxHeight)) / 2;
            var nameBoxY = planeY + PlaneSize + gap;

            var sb = new StringBuilder();
            sb.AppendLine("^XA");
            sb.AppendLine("^CI28");
            // Reset label home / shift left over by a previous courier label.
            sb.AppendLine("^LH0,0");
            sb.AppendLine("^LS0");
            sb.AppendLine($"^PW{LabelWidth}");
            sb.AppendLine($"^LL{LabelHeight}");

            // Heavy frame around the whole label
            sb.AppendLine($"^FO{margin},{margin}^GB{innerWidth},{LabelHeight - 2 * margin},{frame}^FS");

            // Top black bar with reversed caption
            sb.AppendLine($"^FO{margin},{margin}^GB{innerWidth},{barHeight},{barHeight}^FS");
            sb.AppendLine($"^CF0,70");
            sb.AppendLine($"^FO{margin},{margin + (barHeight - 70) / 2}^FB{innerWidth},1,0,C,0^FR^FDPRZESYLKA LOTNICZA^FS");

            // Plane logo
            sb.AppendLine($"^FO{(LabelWidth - PlaneSize) / 2},{planeY}^GFA,{PlaneTotalBytes},{PlaneTotalBytes},{PlaneBytesPerRow},{PlaneGraphic}^FS");

            // Courier name in a framed box in the middle
            sb.AppendLine($"^FO{margin + 40},{nameBoxY}^GB{nameBoxWidth},{nameBoxHeight},8^FS");
            sb.AppendLine($"^CF0,{nameFont}");
            sb.AppendLine($"^FO{margin + 40},{nameBoxY + (nameBoxHeight - nameFont) / 2 + nameFont / 10}^FB{nameBoxWidth},1,0,C,0^FD{name}^FS");

            sb.AppendLine("^XZ");
            return sb.ToString();
        }
    }
}
