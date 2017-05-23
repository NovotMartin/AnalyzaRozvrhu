using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyzaRozvrhu.STAG_Classes;
using System.Diagnostics;

namespace AnalyzaRozvrhu
{
    public static class STAG_DataAnalyzator
    {
        public static void Analyzuj(this STAG_Classes.STAG_Database data)
        {
            // TODO
            Fandova_metoda(data);
            // analyzuje, upravi objektz studenta a pod...
        }

        private static void Fandova_metoda(STAG_Database data)
        {
           foreach(var student in data.Students)
            {
                List<Predmet> kredityLS = new List<Predmet>();
                List<Predmet> kredityZS = new List<Predmet>();
                int maxKredituZS = 0;
                int maxKredituLS = 0;
                foreach (var akce in student.Rozvrh)
                {
                    if (akce.Semestr == "LS")
                    {
                        if (!kredityLS.Contains(akce.PredmetRef))
                            kredityLS.Add(akce.PredmetRef);
                    }
                    else
                    {
                        if (!kredityZS.Contains(akce.PredmetRef))
                            kredityZS.Add(akce.PredmetRef);
                    }
                }
                maxKredituZS = (from predmet in kredityZS select predmet.Kreditu).Sum();
                maxKredituLS = (from predmet in kredityLS select predmet.Kreditu).Sum();
                if (maxKredituZS == 0)
                    continue;
                SpoctiPodil(student, kredityZS, maxKredituZS+maxKredituLS);
                if (maxKredituLS == 0)
                    continue;
                SpoctiPodil(student, kredityLS, maxKredituZS+maxKredituLS);
                Debug.WriteLine("Hloupá analýza hotavá");
            }
        }

        private static void SpoctiPodil(Student student, List<Predmet> kredity, int maxKreditu)
        {
            foreach (var predmet in kredity)
            {
               
                    
                double podilPredmetu = (predmet.Kreditu / (double)maxKreditu * 100);
                int n = 0;
                if (predmet.JednotekCviceni != 0)
                    n++;
                if (predmet.JednotekPrednasek != 0)
                    n++;
                if (predmet.JednotekSeminare != 0)
                    n++;

                if (predmet.JednotekCviceni != 0)
                    foreach (var katedra in predmet.PodilKatedryCviceni)
                    {
                        if(!student.PodilKatedry.ContainsKey(katedra.Key))
                            student.PodilKatedry.Add(katedra.Key, 0);
                        student.PodilKatedry[katedra.Key] += podilPredmetu * (katedra.Value/n);
                    }
                       
                if (predmet.JednotekPrednasek != 0)
                    foreach (var katedra in predmet.PodilKatedryPrednaska)
                    {
                        if (!student.PodilKatedry.ContainsKey(katedra.Key))
                            student.PodilKatedry.Add(katedra.Key, 0);
                        student.PodilKatedry[katedra.Key] += podilPredmetu * (katedra.Value / n);
                    }
                        
                if (predmet.JednotekSeminare != 0)
                    foreach (var katedra in predmet.PodilKatedrySeminar)
                    {
                        if (!student.PodilKatedry.ContainsKey(katedra.Key))
                            student.PodilKatedry.Add(katedra.Key, 0);
                        student.PodilKatedry[katedra.Key] += podilPredmetu * (katedra.Value / n);
                    }
                                   
            }
        }
    }
}
