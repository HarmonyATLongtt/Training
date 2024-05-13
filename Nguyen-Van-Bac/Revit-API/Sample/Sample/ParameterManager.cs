using Autodesk.Revit.DB;

public class ParameterManager
{
    public void AddParameter(Document doc,
        ExternalDefinition familyDefinition,
        BuiltInParameterGroup parameterGroup,
        bool isInstance)
    {
        // Lấy thông tin về đối tượng Document hiện tại

        // Bắt đầu một giao dịch mới
        using (Transaction trans = new Transaction(doc))
        {
            trans.Start("Add Parameter");

            // Tạo một thông số gia đình mới với định nghĩa ngoại (external definition)
            FamilyParameter parameter = doc.FamilyManager.AddParameter(
                familyDefinition,
                parameterGroup,
                isInstance);

            // Kết thúc giao dịch
            trans.Commit();
        }
    }
}