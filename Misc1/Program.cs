﻿// var output = "D:\\SphereDev\\SphereSource\\source\\packets\\mainhand_equip_fists";
//
// var input = Console.ReadLine();
//
// if (input != null)
// {
//     var bytes = Convert.FromHexString(input);
//     File.WriteAllBytes(output, bytes);
// }

using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var win1251 = Encoding.GetEncoding(1251);

var text = "1A00ADB0910F2C0100D6EB596F084043008120003A0000400000D23834DBD09EF237BA0CF173B22271429A50648232D57CAD994A476EBE37B9D02E2CBD2B4D0E3E9732EFC6666E84273BA8833F9C6185EC9418C820DBF671ED2CD4A99F8355CAEC72E1ED5FE127F96501D0170DA91564F3B8C415B4BF96F1F4A534624B03B74818E5C85C71DAD6A77B8510ED925725A9CFFF8082CCD151F5D318D351ECDC7F1BBF6AC5C66E09A3DD72FB6C52D5F0CE8A48F2887F3A3E623B9AF9746594768FE7F333C852684EF55EB6FD03993E47F0A69D38FCC597D88AA887DB09DEF64C5D4EB685DBA1BF141DCA4FF14FF65D9702792C09942DF4402C29A11BA2F9F9050D4572C96B59FC5D85383DE1BC4565B0888D676163960F4BEBFB08525036D41EEDD9B66B2236EDC8E79F4A4590361CE47F4C9F5AB029705254596A8C80D78F7D9BA52E2C8E14FE96DC3CBD5AE65124E5FD90C15FC14D952EF7F988453526B53E2FFC69EC4F3641024BB9EF7A6B5FE4E81D07C8072E906C1C7A88784FECA2E76F2FE859CA35010C824299714DBA769E408E1392C7BB7E375ED921AE926538E5623E32FF0B4F5C4DD1BC788F6F6B09AAFF4F0DFEC8669187299F2F43550BB8C77A13C19415D1870C0182B31D9E6559D734936282313E6A234C2637C18999D806880EBA445484CCE0E8823FC27E1537E4C79721EE1DA893F245026925B4CE4A7902980BDC169502C2118941CAA7EEBAA53C9ADECC2D8F2DF0021143EE63F0CB";
var bytes = Convert.FromHexString(text);
var text1251 = new List<byte>();

for (var i = 1; i < bytes.Length; i++)
{
    var letter = ((bytes[i] & 0b11) << 5) + (bytes[i - 1] >> 3);
    text1251.Add((byte)letter);
}

Console.WriteLine(new string (win1251.GetChars(text1251.ToArray())));