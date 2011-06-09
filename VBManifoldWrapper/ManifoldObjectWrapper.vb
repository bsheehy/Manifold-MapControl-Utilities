Public Class ManifoldObjectWrapper
    Public Shared Function getBranch(ByVal p_BranchSet As Manifold.Interop.BranchSet, ByVal p_iIndex As Integer) As Manifold.Interop.Branch
        Return p_BranchSet(p_iIndex)
    End Function

    Public Shared Function getBranchPointSet(ByVal p_BranchSet As Manifold.Interop.BranchSet, ByVal p_iIndex As Integer) As Manifold.Interop.PointSet
        Return p_BranchSet(p_iIndex).PointSet
    End Function

    Public Shared Function getPointFromPointSet(ByVal p_PointSet As Manifold.Interop.PointSet, ByVal p_iIndex As Integer) As Manifold.Interop.Point
        Return p_PointSet(p_iIndex)
    End Function

    Public Shared Function projectPointSetToNativeCoords(ByVal p_PointSet As Manifold.Interop.PointSet, ByVal p_MapControl As AxManifold.Interop.AxComponentControl) As Manifold.Interop.PointSet
        For i As Integer = 0 To p_PointSet.Count - 1
            p_PointSet(i) = p_MapControl.ScreenToNative(p_PointSet(i))
        Next
        Return p_PointSet
    End Function

    Public Shared Sub setPointInPointSet(ByVal p_PointSet As Manifold.Interop.PointSet, ByVal p_Point As Manifold.Interop.Point, ByVal p_iIndex As Integer)
        p_PointSet(p_iIndex) = p_Point
    End Sub

    Public Shared Sub setPointSetInBranchSet(ByVal p_PointSet As Manifold.Interop.PointSet, ByVal p_BranchSet As Manifold.Interop.BranchSet)
        p_BranchSet.Add(p_PointSet)
    End Sub

End Class
