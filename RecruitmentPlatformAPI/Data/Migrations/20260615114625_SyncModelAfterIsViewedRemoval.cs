using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterIsViewedRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 2,
                column: "TitleAr",
                value: "مطور واجهات أمامية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 3,
                column: "TitleAr",
                value: "مطور شامل");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 5,
                column: "TitleAr",
                value: "مطور iOS");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 6,
                column: "TitleAr",
                value: "مطور أندرويد");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 10,
                column: "TitleAr",
                value: "مهندس تعلم الآلة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 14,
                column: "TitleAr",
                value: "مهندس أتمتة الاختبارات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 15,
                column: "TitleAr",
                value: "مهندس حوسبة سحابية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 19,
                column: "TitleAr",
                value: "مسؤول أنظمة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 20,
                column: "TitleAr",
                value: "مسؤول قواعد بيانات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 21,
                column: "TitleAr",
                value: "مهندس حلول");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 22,
                column: "TitleAr",
                value: "مهندس بنية تقنية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 23,
                column: "TitleAr",
                value: "مهندس موثوقية الموقع");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 26,
                column: "TitleAr",
                value: "مطور بلوك تشين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 27,
                column: "TitleAr",
                value: "مهندس إنترنت الأشياء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 29,
                column: "TitleAr",
                value: "مهندس معالجة اللغات الطبيعية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 33,
                column: "TitleAr",
                value: "مهندس دعم فني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 35,
                column: "TitleAr",
                value: "الرئيس التنفيذي للتكنولوجيا (CTO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 36,
                column: "TitleAr",
                value: "مدير هندسي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 37,
                column: "TitleAr",
                value: "قائد تقني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 38,
                column: "TitleAr",
                value: "سكرم ماستر");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 43,
                column: "TitleAr",
                value: "مصمم UI/UX");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 45,
                column: "TitleAr",
                value: "مصمم ويب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 46,
                column: "TitleAr",
                value: "مصمم مرئي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 48,
                column: "TitleAr",
                value: "مصمم تفاعلي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 49,
                column: "TitleAr",
                value: "مصمم موشن جرافيك");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 53,
                column: "TitleAr",
                value: "مدير إبداعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 54,
                column: "TitleAr",
                value: "مدير فني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 55,
                column: "TitleAr",
                value: "مصمم هوية بصرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 57,
                column: "TitleAr",
                value: "أخصائي SEO");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 58,
                column: "TitleAr",
                value: "مدير تسويق بالمحتوى");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 61,
                column: "TitleAr",
                value: "مدير علامة تجارية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 62,
                column: "TitleAr",
                value: "مدير نمو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 63,
                column: "TitleAr",
                value: "أخصائي تسويق عبر البريد الإلكتروني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 67,
                column: "TitleAr",
                value: "مسؤول حسابات تنفيذي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 69,
                column: "TitleAr",
                value: "مدير تطوير أعمال");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 70,
                column: "TitleAr",
                value: "مدير نجاح عملاء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 74,
                column: "TitleAr",
                value: "الرئيس التنفيذي للمالية (CFO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 75,
                column: "TitleAr",
                value: "محلل استثمار");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 76,
                column: "TitleAr",
                value: "مدير موارد بشرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 78,
                column: "TitleAr",
                value: "أخصائي استقطاب مواهب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 79,
                column: "TitleAr",
                value: "شريك أعمال موارد بشرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 81,
                column: "TitleAr",
                value: "مدير عمليات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 84,
                column: "TitleAr",
                value: "مدير سلاسل الإمداد");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 85,
                column: "TitleAr",
                value: "منسق لوجستي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 87,
                column: "TitleAr",
                value: "الرئيس التنفيذي للعمليات (COO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 89,
                column: "TitleAr",
                value: "نائب رئيس المنتجات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 91,
                column: "TitleAr",
                value: "مهندس منصات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 93,
                column: "TitleAr",
                value: "معماري سحابي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 94,
                column: "TitleAr",
                value: "معماري أمن معلومات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 96,
                column: "TitleAr",
                value: "محلل SOC");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 97,
                column: "TitleAr",
                value: "مهندس MLOps");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 98,
                column: "TitleAr",
                value: "معماري بيانات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 100,
                column: "TitleAr",
                value: "مهندس أوامر (Prompt Engineer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 101,
                column: "TitleAr",
                value: "مطور AR/VR");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 102,
                column: "TitleAr",
                value: "مهندس برامج ثابتة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 103,
                column: "TitleAr",
                value: "مطور RPA");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 104,
                column: "TitleAr",
                value: "مهندس Staff");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 106,
                column: "TitleAr",
                value: "معماري برمجيات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 107,
                column: "TitleAr",
                value: "مهندس تكامل أنظمة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 108,
                column: "TitleAr",
                value: "مطور Salesforce");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 109,
                column: "TitleAr",
                value: "مطور ERP");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 110,
                column: "TitleAr",
                value: "مطور SharePoint");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 111,
                column: "TitleAr",
                value: "مهندس API");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 112,
                column: "TitleAr",
                value: "معماري خدمات مصغرة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 114,
                column: "TitleAr",
                value: "مطور Web3");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 115,
                column: "TitleAr",
                value: "معماري منصات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 116,
                column: "TitleAr",
                value: "معماري حلول AWS");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 117,
                column: "TitleAr",
                value: "مهندس بناء وإصدار");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 118,
                column: "TitleAr",
                value: "مدير إصدارات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 119,
                column: "TitleAr",
                value: "مهندس مراقبة أنظمة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 120,
                column: "TitleAr",
                value: "مهندس أداء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 121,
                column: "TitleAr",
                value: "مهندس أتمتة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 122,
                column: "TitleAr",
                value: "عالم أبحاث ذكاء اصطناعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 125,
                column: "TitleAr",
                value: "محلل أدلة جنائية رقمية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 126,
                column: "TitleAr",
                value: "محلل استجابة للحوادث");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 127,
                column: "TitleAr",
                value: "باحث ثغرات أمنية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 128,
                column: "TitleAr",
                value: "مدير عمليات أمنية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 129,
                column: "TitleAr",
                value: "مطور Low-Code/No-Code");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 130,
                column: "TitleAr",
                value: "مدير برنامج تقني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 131,
                column: "TitleAr",
                value: "كاتب تجربة مستخدم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 132,
                column: "TitleAr",
                value: "قائد أنظمة تصميم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 134,
                column: "TitleAr",
                value: "محرر فيديو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 139,
                column: "TitleAr",
                value: "مدير تصميم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 140,
                column: "TitleAr",
                value: "مهندس UI");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 141,
                column: "TitleAr",
                value: "مصمم تايبوجرافي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 142,
                column: "TitleAr",
                value: "مدير تسويق بالأداء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 145,
                column: "TitleAr",
                value: "مدير تسويق عبر المؤثرين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 146,
                column: "TitleAr",
                value: "مدير تسويق بالعمولة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 147,
                column: "TitleAr",
                value: "مدير علاقات عامة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 148,
                column: "TitleAr",
                value: "مدير حملات تسويقية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 149,
                column: "TitleAr",
                value: "مدير عمليات تسويق");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 150,
                column: "TitleAr",
                value: "مدير مجتمع");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 151,
                column: "TitleAr",
                value: "مدير تسويق نمو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 152,
                column: "TitleAr",
                value: "ممثل تطوير مبيعات (SDR)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 153,
                column: "TitleAr",
                value: "ممثل تطوير أعمال (BDR)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 154,
                column: "TitleAr",
                value: "مسؤول حسابات مؤسسية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 155,
                column: "TitleAr",
                value: "مدير حسابات رئيسية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 156,
                column: "TitleAr",
                value: "مدير مبيعات قنوات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 157,
                column: "TitleAr",
                value: "مدير عمليات مبيعات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 160,
                column: "TitleAr",
                value: "مهندس حلول");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 161,
                column: "TitleAr",
                value: "مدير إيرادات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 163,
                column: "TitleAr",
                value: "مدير خزينة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 164,
                column: "TitleAr",
                value: "أخصائي ضرائب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 167,
                column: "TitleAr",
                value: "مسؤول امتثال");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 168,
                column: "TitleAr",
                value: "أخصائي رواتب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 169,
                column: "TitleAr",
                value: "محلل ميزانية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 171,
                column: "TitleAr",
                value: "مدير تخطيط وتحليل مالي (FP&A)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 172,
                column: "TitleAr",
                value: "مدير تعلم وتطوير");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 173,
                column: "TitleAr",
                value: "مدير تعويضات ومزايا");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 174,
                column: "TitleAr",
                value: "مدير علاقات موظفين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 176,
                column: "TitleAr",
                value: "محلل قوى عاملة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 177,
                column: "TitleAr",
                value: "أخصائي تهيئة موظفين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 178,
                column: "TitleAr",
                value: "مدير إدارة موارد بشرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 179,
                column: "TitleAr",
                value: "الرئيس التنفيذي للموارد البشرية (CPO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 180,
                column: "TitleAr",
                value: "مدير ثقافة وتفاعل مؤسسي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 181,
                column: "TitleAr",
                value: "أخصائي تطوير تنظيمي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 183,
                column: "TitleAr",
                value: "مدير تحسين عمليات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 184,
                column: "TitleAr",
                value: "مدير مشتريات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 185,
                column: "TitleAr",
                value: "مدير موردين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 186,
                column: "TitleAr",
                value: "مدير دعم عملاء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 187,
                column: "TitleAr",
                value: "أخصائي دعم عملاء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 189,
                column: "TitleAr",
                value: "كبير موظفين (Chief of Staff)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 190,
                column: "TitleAr",
                value: "مدير استراتيجية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 191,
                column: "TitleAr",
                value: "مدير ERP");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 192,
                column: "TitleAr",
                value: "مدير عمليات إيرادات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 194,
                column: "TitleAr",
                value: "مدير تحول مؤسسي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 196,
                column: "TitleAr",
                value: "مدير مرافق");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 200,
                column: "TitleAr",
                value: "مدير إدارة منتجات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 201,
                column: "TitleAr",
                value: "رئيس قسم المنتجات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 202,
                column: "TitleAr",
                value: "مالك منتج");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 204,
                column: "TitleAr",
                value: "مدير عمليات المنتجات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 205,
                column: "TitleAr",
                value: "مدير منتج منصة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 206,
                column: "TitleAr",
                value: "مدير منتج نمو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 207,
                column: "TitleAr",
                value: "مدير منتج ذكاء اصطناعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 210,
                column: "TitleAr",
                value: "صانع محتوى");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 211,
                column: "TitleAr",
                value: "كاتب إعلانات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 214,
                column: "TitleAr",
                value: "منتج بودكاست");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 215,
                column: "TitleAr",
                value: "صانع محتوى تواصل اجتماعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 218,
                column: "TitleAr",
                value: "استراتيجي محتوى");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 221,
                column: "TitleAr",
                value: "مدير اتصالات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 222,
                column: "TitleAr",
                value: "أخصائي توثيق");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 223,
                column: "TitleAr",
                value: "كاتب مقترحات تمويل");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 225,
                column: "TitleAr",
                value: "الرئيس التنفيذي للتسويق (CMO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 226,
                column: "TitleAr",
                value: "الرئيس التنفيذي للإيرادات (CRO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 227,
                column: "TitleAr",
                value: "الرئيس التنفيذي للمنتجات (CPO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 228,
                column: "TitleAr",
                value: "الرئيس التنفيذي لتقنية المعلومات (CIO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 229,
                column: "TitleAr",
                value: "الرئيس التنفيذي للبيانات (CDO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 230,
                column: "TitleAr",
                value: "الرئيس التنفيذي لأمن المعلومات (CSO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 231,
                column: "TitleAr",
                value: "الرئيس التنفيذي للذكاء الاصطناعي (CAIO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 232,
                column: "TitleAr",
                value: "مدير تنفيذي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 238,
                column: "TitleAr",
                value: "رئيس حسابات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 242,
                column: "TitleAr",
                value: "رئيس القسم المالي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 244,
                column: "TitleAr",
                value: "أخصائي أنظمة موارد بشرية (HRIS)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 248,
                column: "TitleAr",
                value: "مدير إدارة العمليات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 249,
                column: "TitleAr",
                value: "مدير مستودع");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 250,
                column: "TitleAr",
                value: "مدير أسطول");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 252,
                column: "TitleAr",
                value: "أخصائي PPC");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 254,
                column: "TitleAr",
                value: "باحث مستخدمين");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 2,
                column: "TitleAr",
                value: "مطور واجهات أمامية (Front-End Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 3,
                column: "TitleAr",
                value: "مطور مكدس كامل (Full-Stack Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 5,
                column: "TitleAr",
                value: "مطور تطبيقات iOS");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 6,
                column: "TitleAr",
                value: "مطور تطبيقات أندرويد");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 10,
                column: "TitleAr",
                value: "مهندس تعلم آلي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 14,
                column: "TitleAr",
                value: "مهندس أتمتة الاختبار");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 15,
                column: "TitleAr",
                value: "مهندس الحوسبة السحابية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 19,
                column: "TitleAr",
                value: "مدير أنظمة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 20,
                column: "TitleAr",
                value: "مدير قواعد البيانات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 21,
                column: "TitleAr",
                value: "مهندس حلول معمارية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 22,
                column: "TitleAr",
                value: "مهندس معماري تقني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 23,
                column: "TitleAr",
                value: "مهندس موثوقية الموقع (SRE)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 26,
                column: "TitleAr",
                value: "مطور بلوك تشين (Blockchain Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 27,
                column: "TitleAr",
                value: "مهندس إنترنت الأشياء (IoT)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 29,
                column: "TitleAr",
                value: "مهندس معالجة اللغات الطبيعية (NLP)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 33,
                column: "TitleAr",
                value: "مهندس الدعم الفني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 35,
                column: "TitleAr",
                value: "المدير التنفيذي للتكنولوجيا (CTO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 36,
                column: "TitleAr",
                value: "مدير هندسة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 37,
                column: "TitleAr",
                value: "القائد التقني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 38,
                column: "TitleAr",
                value: "سكرم ماستر (Scrum Master)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 43,
                column: "TitleAr",
                value: "مصمم تجربة وواجهة المستخدم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 45,
                column: "TitleAr",
                value: "مصمم مواقع ويب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 46,
                column: "TitleAr",
                value: "مصمم بصري");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 48,
                column: "TitleAr",
                value: "مصمم التفاعل");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 49,
                column: "TitleAr",
                value: "مصمم الحركة والمؤثرات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 53,
                column: "TitleAr",
                value: "المدير الإبداعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 54,
                column: "TitleAr",
                value: "المدير الفني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 55,
                column: "TitleAr",
                value: "مصمم الهوية البصرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 57,
                column: "TitleAr",
                value: "أخصائي تحسين محركات البحث (SEO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 58,
                column: "TitleAr",
                value: "مدير تسويق المحتوى");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 61,
                column: "TitleAr",
                value: "مدير العلامة التجارية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 62,
                column: "TitleAr",
                value: "مدير النمو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 63,
                column: "TitleAr",
                value: "أخصائي التسويق عبر البريد الإلكتروني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 67,
                column: "TitleAr",
                value: "مسؤول تنفيذي للحسابات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 69,
                column: "TitleAr",
                value: "مدير تطوير الأعمال");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 70,
                column: "TitleAr",
                value: "مدير نجاح العملاء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 74,
                column: "TitleAr",
                value: "المدير المالي التنفيذي (CFO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 75,
                column: "TitleAr",
                value: "محلل استثماري");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 76,
                column: "TitleAr",
                value: "مدير الموارد البشرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 78,
                column: "TitleAr",
                value: "أخصائي اكتساب المواهب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 79,
                column: "TitleAr",
                value: "شريك أعمال الموارد البشرية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 81,
                column: "TitleAr",
                value: "مدير العمليات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 84,
                column: "TitleAr",
                value: "مدير سلسلة التوريد");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 85,
                column: "TitleAr",
                value: "منسق لوجستيات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 87,
                column: "TitleAr",
                value: "المدير التنفيذي للعمليات (COO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 89,
                column: "TitleAr",
                value: "نائب رئيس المنتج");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 91,
                column: "TitleAr",
                value: "مهندس منصة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 93,
                column: "TitleAr",
                value: "مهندس معماري سحابي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 94,
                column: "TitleAr",
                value: "مهندس معماري أمن المعلومات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 96,
                column: "TitleAr",
                value: "محلل عمليات أمن المعلومات (SOC)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 97,
                column: "TitleAr",
                value: "مهندس عمليات تعلم آلي (MLOps)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 98,
                column: "TitleAr",
                value: "مهندس معماري البيانات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 100,
                column: "TitleAr",
                value: "مهندس مطالبات الذكاء الاصطناعي (Prompt Engineer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 101,
                column: "TitleAr",
                value: "مطور الواقع المعزز والافتراضي (AR/VR Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 102,
                column: "TitleAr",
                value: "مهندس البرامج الثابتة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 103,
                column: "TitleAr",
                value: "مطور أتمتة العمليات الروبوتية (RPA Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 104,
                column: "TitleAr",
                value: "مهندس متخصص أول");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 106,
                column: "TitleAr",
                value: "مهندس معماري برمجيات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 107,
                column: "TitleAr",
                value: "مهندس تكامل الأنظمة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 108,
                column: "TitleAr",
                value: "مطور سيلزفورس (Salesforce Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 109,
                column: "TitleAr",
                value: "مطور أنظمة تخطيط الموارد (ERP)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 110,
                column: "TitleAr",
                value: "مطور شيربوينت (SharePoint Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 111,
                column: "TitleAr",
                value: "مهندس واجهات برمجية (API)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 112,
                column: "TitleAr",
                value: "مهندس معماري الخدمات المصغرة (Microservices)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 114,
                column: "TitleAr",
                value: "مطور ويب 3 (Web3 Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 115,
                column: "TitleAr",
                value: "مهندس معماري المنصات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 116,
                column: "TitleAr",
                value: "مهندس حلول AWS");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 117,
                column: "TitleAr",
                value: "مهندس البناء والإصدار");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 118,
                column: "TitleAr",
                value: "مدير الإصدارات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 119,
                column: "TitleAr",
                value: "مهندس مراقبة الأنظمة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 120,
                column: "TitleAr",
                value: "مهندس الأداء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 121,
                column: "TitleAr",
                value: "مهندس الأتمتة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 122,
                column: "TitleAr",
                value: "عالم أبحاث الذكاء الاصطناعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 125,
                column: "TitleAr",
                value: "محلل الجنائيات الرقمية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 126,
                column: "TitleAr",
                value: "محلل الاستجابة للحوادث");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 127,
                column: "TitleAr",
                value: "باحث الثغرات الأمنية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 128,
                column: "TitleAr",
                value: "مدير عمليات أمن المعلومات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 129,
                column: "TitleAr",
                value: "مطور منخفض الكود أو بدون كود (Low-Code/No-Code Developer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 130,
                column: "TitleAr",
                value: "مدير برامج تقنية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 131,
                column: "TitleAr",
                value: "كاتب تجربة المستخدم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 132,
                column: "TitleAr",
                value: "قائد أنظمة التصميم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 134,
                column: "TitleAr",
                value: "مونتير فيديو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 139,
                column: "TitleAr",
                value: "مدير التصميم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 140,
                column: "TitleAr",
                value: "مهندس واجهات المستخدم");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 141,
                column: "TitleAr",
                value: "خطاط رقمي ومصمم طباعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 142,
                column: "TitleAr",
                value: "مدير التسويق بالأداء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 145,
                column: "TitleAr",
                value: "مدير التسويق عبر المؤثرين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 146,
                column: "TitleAr",
                value: "مدير التسويق بالعمولة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 147,
                column: "TitleAr",
                value: "مدير العلاقات العامة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 148,
                column: "TitleAr",
                value: "مدير الحملات التسويقية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 149,
                column: "TitleAr",
                value: "مدير عمليات التسويق");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 150,
                column: "TitleAr",
                value: "مدير المجتمع");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 151,
                column: "TitleAr",
                value: "مدير تسويق النمو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 152,
                column: "TitleAr",
                value: "ممثل تطوير المبيعات (SDR)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 153,
                column: "TitleAr",
                value: "ممثل تطوير الأعمال (BDR)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 154,
                column: "TitleAr",
                value: "مسؤول حسابات المؤسسات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 155,
                column: "TitleAr",
                value: "مدير الحسابات الرئيسية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 156,
                column: "TitleAr",
                value: "مدير المبيعات عبر القنوات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 157,
                column: "TitleAr",
                value: "مدير عمليات المبيعات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 160,
                column: "TitleAr",
                value: "مهندس الحلول");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 161,
                column: "TitleAr",
                value: "مدير الإيرادات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 163,
                column: "TitleAr",
                value: "مدير الخزينة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 164,
                column: "TitleAr",
                value: "أخصائي الضرائب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 167,
                column: "TitleAr",
                value: "مسؤول الامتثال");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 168,
                column: "TitleAr",
                value: "أخصائي الرواتب");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 169,
                column: "TitleAr",
                value: "محلل الميزانية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 171,
                column: "TitleAr",
                value: "مدير التخطيط المالي والتحليل (FP&A)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 172,
                column: "TitleAr",
                value: "مدير التعلم والتطوير");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 173,
                column: "TitleAr",
                value: "مدير التعويضات والمزايا");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 174,
                column: "TitleAr",
                value: "مدير علاقات الموظفين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 176,
                column: "TitleAr",
                value: "محلل القوى العاملة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 177,
                column: "TitleAr",
                value: "أخصائي الاستيعاب الوظيفي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 178,
                column: "TitleAr",
                value: "مدير الموارد البشرية التنفيذي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 179,
                column: "TitleAr",
                value: "كبير مسؤولي الأفراد");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 180,
                column: "TitleAr",
                value: "مدير الثقافة والتفاعل المؤسسي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 181,
                column: "TitleAr",
                value: "أخصائي التطوير التنظيمي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 183,
                column: "TitleAr",
                value: "مدير تحسين العمليات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 184,
                column: "TitleAr",
                value: "مدير المشتريات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 185,
                column: "TitleAr",
                value: "مدير الموردين");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 186,
                column: "TitleAr",
                value: "مدير دعم العملاء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 187,
                column: "TitleAr",
                value: "أخصائي دعم العملاء");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 189,
                column: "TitleAr",
                value: "رئيس موظفي الإدارة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 190,
                column: "TitleAr",
                value: "مدير الاستراتيجية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 191,
                column: "TitleAr",
                value: "مدير أنظمة تخطيط الموارد (ERP)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 192,
                column: "TitleAr",
                value: "مدير عمليات الإيرادات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 194,
                column: "TitleAr",
                value: "مدير التحول المؤسسي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 196,
                column: "TitleAr",
                value: "مدير المرافق والخدمات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 200,
                column: "TitleAr",
                value: "مدير إدارة المنتجات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 201,
                column: "TitleAr",
                value: "رئيس قسم المنتج");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 202,
                column: "TitleAr",
                value: "مالك المنتج");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 204,
                column: "TitleAr",
                value: "مدير عمليات المنتج");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 205,
                column: "TitleAr",
                value: "مدير منتج المنصة");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 206,
                column: "TitleAr",
                value: "مدير منتج النمو");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 207,
                column: "TitleAr",
                value: "مدير منتج الذكاء الاصطناعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 210,
                column: "TitleAr",
                value: "منشئ محتوى");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 211,
                column: "TitleAr",
                value: "كاتب إعلاني");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 214,
                column: "TitleAr",
                value: "منتج بودكاست (Podcast Producer)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 215,
                column: "TitleAr",
                value: "منشئ محتوى وسائل التواصل الاجتماعي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 218,
                column: "TitleAr",
                value: "استراتيجي المحتوى");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 221,
                column: "TitleAr",
                value: "مدير الاتصالات");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 222,
                column: "TitleAr",
                value: "أخصائي التوثيق");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 223,
                column: "TitleAr",
                value: "كاتب مقترحات التمويل");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 225,
                column: "TitleAr",
                value: "كبير مسؤولي التسويق (CMO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 226,
                column: "TitleAr",
                value: "كبير مسؤولي الإيرادات (CRO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 227,
                column: "TitleAr",
                value: "كبير مسؤولي المنتجات (CPO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 228,
                column: "TitleAr",
                value: "كبير مسؤولي تقنية المعلومات (CIO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 229,
                column: "TitleAr",
                value: "كبير مسؤولي البيانات (CDO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 230,
                column: "TitleAr",
                value: "كبير مسؤولي أمن المعلومات (CSO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 231,
                column: "TitleAr",
                value: "كبير مسؤولي الذكاء الاصطناعي (CAIO)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 232,
                column: "TitleAr",
                value: "المدير التنفيذي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 238,
                column: "TitleAr",
                value: "محاسب رئيسي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 242,
                column: "TitleAr",
                value: "رئيس قسم المالية");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 244,
                column: "TitleAr",
                value: "أخصائي أنظمة معلومات الموارد البشرية (HRIS)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 248,
                column: "TitleAr",
                value: "مدير العمليات التنفيذي");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 249,
                column: "TitleAr",
                value: "مدير المستودع");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 250,
                column: "TitleAr",
                value: "مدير الأسطول");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 252,
                column: "TitleAr",
                value: "أخصائي إعلانات الدفع مقابل النقر (PPC)");

            migrationBuilder.UpdateData(
                table: "JobTitle",
                keyColumn: "Id",
                keyValue: 254,
                column: "TitleAr",
                value: "باحث المستخدمين");
        }
    }
}
