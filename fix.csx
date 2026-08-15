using System;
using System.IO;

var filePath = @"D:\ITI 9 Month\Graduation Project\SmartCourt\SmartCourt\Persistence\Migrations\20260815134657_MigrateDateTimeToDateTimeOffset.cs";
var content = File.ReadAllText(filePath);

string withdrawalUpOld = @"            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: ""ManualActionRequiredAt"",
                table: ""WithdrawalRequests""," ;
string withdrawalUpNew = @"            migrationBuilder.DropCheckConstraint(
                name: ""CK_WithdrawalRequests_ManualActionTimestamp"",
                table: ""WithdrawalRequests"");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: ""ManualActionRequiredAt"",
                table: ""WithdrawalRequests""," ;

if (!content.Contains(""CK_WithdrawalRequests_ManualActionTimestamp""))
{
    content = content.Replace(withdrawalUpOld, withdrawalUpNew);
}

string paymentUpOld = @"            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: ""ManualActionRequiredAt"",
                table: ""PaymentTransactions""," ;
string paymentUpNew = @"            migrationBuilder.DropCheckConstraint(
                name: ""CK_PaymentTransactions_ManualActionTimestamp"",
                table: ""PaymentTransactions"");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: ""ManualActionRequiredAt"",
                table: ""PaymentTransactions""," ;

if (!content.Contains(""CK_PaymentTransactions_ManualActionTimestamp""))
{
    content = content.Replace(paymentUpOld, paymentUpNew);
}


string withdrawalDownOld = @"            migrationBuilder.AlterColumn<DateTime>(
                name: ""ManualActionRequiredAt"",
                table: ""WithdrawalRequests""," ;
string withdrawalDownNew = @"            migrationBuilder.DropCheckConstraint(
                name: ""CK_WithdrawalRequests_ManualActionTimestamp"",
                table: ""WithdrawalRequests"");

            migrationBuilder.AlterColumn<DateTime>(
                name: ""ManualActionRequiredAt"",
                table: ""WithdrawalRequests""," ;

content = content.Replace(withdrawalDownOld, withdrawalDownNew);

string paymentDownOld = @"            migrationBuilder.AlterColumn<DateTime>(
                name: ""ManualActionRequiredAt"",
                table: ""PaymentTransactions""," ;
string paymentDownNew = @"            migrationBuilder.DropCheckConstraint(
                name: ""CK_PaymentTransactions_ManualActionTimestamp"",
                table: ""PaymentTransactions"");

            migrationBuilder.AlterColumn<DateTime>(
                name: ""ManualActionRequiredAt"",
                table: ""PaymentTransactions""," ;

content = content.Replace(paymentDownOld, paymentDownNew);

string addConstraintsUp = @"            migrationBuilder.AddCheckConstraint(
                name: ""CK_WithdrawalRequests_ManualActionTimestamp"",
                table: ""WithdrawalRequests"",
                sql: ""[RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL"");

            migrationBuilder.AddCheckConstraint(
                name: ""CK_PaymentTransactions_ManualActionTimestamp"",
                table: ""PaymentTransactions"",
                sql: ""[RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)" ;
content = content.Replace(@"        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)", addConstraintsUp);

string addConstraintsDown = @"            migrationBuilder.AddCheckConstraint(
                name: ""CK_WithdrawalRequests_ManualActionTimestamp"",
                table: ""WithdrawalRequests"",
                sql: ""[RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL"");

            migrationBuilder.AddCheckConstraint(
                name: ""CK_PaymentTransactions_ManualActionTimestamp"",
                table: ""PaymentTransactions"",
                sql: ""[RequiresManualAction] = 0 OR [ManualActionRequiredAt] IS NOT NULL"");
        }
    }
}" ;
content = content.Replace(@"        }
    }
}", addConstraintsDown);

File.WriteAllText(filePath, content);
Console.WriteLine(""Done"");
