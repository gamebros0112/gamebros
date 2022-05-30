using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDFViewController : MonoBehaviour
{
    PDFLoader pdfLoader;
    private void Awake()
    {
        pdfLoader = GetComponent<PDFLoader>();
    }
    // 이전 페이지 
    public void OnPrevPdf()
    {
        pdfLoader.PrevPage();
    }
    // 다음 페이지 
    public void OnNextPdf()
    {
        pdfLoader.NextPage();
    }
}
